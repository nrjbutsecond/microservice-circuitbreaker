using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UserService.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.Items["UserId"];

            if (userId == null)
            {
                context.Result = new JsonResult(
                    ApiResponse<object>.ErrorResponse("Unauthorized"))
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }
        }
    }
}