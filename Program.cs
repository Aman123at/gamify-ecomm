using DotNetEnv;
using System.Security.Claims;
using System.Text;
using GamifyApi.Middlwares;
using GamifyApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Npgsql;
using CloudinaryDotNet;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.ComponentModel;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// logging
builder.Logging.AddConsole();

builder.Services.AddDbContext<GamifyDbContext>(opt =>
{
    opt.UseNpgsql(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ?? "",
    npgsqlOptions => npgsqlOptions.EnableRetryOnFailure());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Swagger services
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gamify API",
        Version = "v1",
        Description = "An e-commerce API for gaming products with authentication, cart management, and order processing.",
    });

    // Configure Bearer token authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Group endpoints by controller
    c.TagActionsBy(api =>
    {
        if (api.GroupName != null)
        {
            return new[] { api.GroupName };
        }

        var controllerActionDescriptor = api.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        if (controllerActionDescriptor != null)
        {
            return new[] { controllerActionDescriptor.ControllerName.Replace("Controller", "") };
        }

        return new[] { api.RelativePath };
    });

    c.DocInclusionPredicate((name, api) => true);
});

builder.Services.AddScoped<AuthService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
    options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET_KEY") ?? "")),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    }
);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("SellerOnly", policy => policy.RequireRole("Seller"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

// Add Cloudinary configuration
builder.Services.AddSingleton(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new Cloudinary(new Account(
        Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME") ?? "",
        Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") ?? "",
        Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") ?? ""));
});

// Listen for RabbitMQ messages in background
builder.Services.AddHostedService<RabbitMQListenerService>();

var app = builder.Build();

// Configure Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gamify API V1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Gamify API Documentation";
        c.DefaultModelsExpandDepth(-1);
    });
}

// Database connection verification
try
{
    var retryPolicy = Policy
        .Handle<NpgsqlException>()
        .WaitAndRetryAsync(
            5,
            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (exception, delay, retryCount, context) =>
            {
                app.Logger.LogWarning(
                    "Retry {RetryCount} due to {ExceptionMessage}",
                    retryCount, exception.Message);
            });

    await retryPolicy.ExecuteAsync(async () =>
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GamifyDbContext>();
        if (!await dbContext.Database.CanConnectAsync())
        {
            throw new Exception("Could not connect to database");
        }
    });
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Application startup failed due to database issues");
    throw;
}

app.UseRouting();
app.UseCors(builder =>
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AdminMiddleware>();
app.UseMiddleware<SellerMiddleware>();

// 4. Add logging to verify pipeline order
app.Use(async (context, next) => {
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Endpoint: {Endpoint}", context.GetEndpoint()?.DisplayName);
    await next();
});


// app.UseHttpsRedirection();

app.MapControllers();

app.Run(Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://localhost:3000");