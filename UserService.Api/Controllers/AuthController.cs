using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.Application.DTOs;
using UserService.Core.Application.Service;

namespace UserService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
            [FromBody] LoginRequest request,
            CancellationToken ct)
        {
            var response = await _authService.LoginAsync(request, ct);
            return Ok(ApiResponse<LoginResponse>.SuccessResponse(response, "Login successful"));
        }
    }
}

