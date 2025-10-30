using Common.Models;
using System.Text.Json;
using UserService.Core.Application.Service;

namespace UserService.Api.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;

        public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IJwtService jwtService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                var userId = jwtService.ValidateToken(token);
                if (userId != null)
                {
                    // Attach user ID to context for use in controllers
                    context.Items["UserId"] = userId.Value;
                    _logger.LogDebug("JWT validated for user: {UserId}", userId);
                }
            }

            await _next(context);
        }
    }
}