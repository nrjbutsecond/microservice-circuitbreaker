namespace ReadingService.API.Middleware;

public class ServiceAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;
    private readonly ILogger<ServiceAuthenticationMiddleware> _logger;

    public ServiceAuthenticationMiddleware(
        RequestDelegate next,
        IConfiguration config,
        ILogger<ServiceAuthenticationMiddleware> logger)
    {
        _next = next;
        _config = config;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for health check, swagger, and circuit breaker monitoring
        var path = context.Request.Path.Value?.ToLower() ?? "";
        if (path.StartsWith("/health") ||
            path.StartsWith("/swagger") ||
            path.StartsWith("/api/circuitbreaker"))
        {
            await _next(context);
            return;
        }

        // Get API Key from header
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
        {
            _logger.LogWarning("🔴 Missing X-API-Key header from {IP}",
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Missing X-API-Key header. Service-to-service authentication required."
            });
            return;
        }

        // Validate API Key
        var allowedServices = _config.GetSection("AllowedServices").GetChildren();
        bool isValid = false;
        string? serviceName = null;

        foreach (var service in allowedServices)
        {
            var expectedKey = service.GetValue<string>("ApiKey");
            if (apiKey == expectedKey)
            {
                isValid = true;
                serviceName = service.Key;
                break;
            }
        }

        if (!isValid)
        {
            _logger.LogWarning("🔴 Invalid API Key from {IP}",
                context.Connection.RemoteIpAddress);

            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Invalid API Key"
            });
            return;
        }

        _logger.LogInformation("✅ Service authenticated: {ServiceName}", serviceName);

        // Store service name in HttpContext for controllers to use
        context.Items["ServiceName"] = serviceName;

        await _next(context);
    }
}
