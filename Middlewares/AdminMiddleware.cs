namespace GamifyApi.Middlwares;

public class AdminMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AdminMiddleware> _logger;
    public AdminMiddleware(RequestDelegate next, ILogger<AdminMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<AdminAttribute>() != null)
        {
            _logger.LogInformation($"AdminMiddleware invoked at {DateTime.UtcNow}");
            if (!context.User.IsInRole("Admin"))
            {
                _logger.LogWarning($"Forbidden access attempt by {context.User.Identity?.Name}");
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Admin access required.");
                return;
            }
        }
        await _next(context);
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AdminAttribute : Attribute
{
}