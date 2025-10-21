using Common.Models;
using Shared.common;
using System.Net;
using System.Text.Json;

namespace UserService.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = exception switch
            {
                NotFoundException notFoundEx => new
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Response = ApiResponse<object>.ErrorResponse(notFoundEx.Message)
                },
                ValidationException validationEx => new
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Response = ApiResponse<object>.ErrorResponse(validationEx.Message, validationEx.Errors)
                },
                _ => new
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Response = ApiResponse<object>.ErrorResponse("An internal error occurred")
                }
            };

            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

            context.Response.StatusCode = (int)response.StatusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(response.Response));
        }
    }
}
