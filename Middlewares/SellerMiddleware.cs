namespace GamifyApi.Middlwares;

public class SellerMiddleware
{
    private readonly RequestDelegate _next;
    public SellerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.GetEndpoint()?.Metadata.GetMetadata<SellerAttribute>() != null)
        {
            if (!context.User.IsInRole("Seller"))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Seller access required.");
                return;
            }
        }
        await _next(context);
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class SellerAttribute : Attribute {}