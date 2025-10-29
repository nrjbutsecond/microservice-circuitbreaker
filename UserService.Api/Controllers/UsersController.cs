using Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UserService.Core.Application.DTOs;
using UserService.Core.Application.Service;

namespace UserService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        
    private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// 🔴 ENDPOINT BỊ GỌI TỪ READING-SERVICE (Circuit Breaker Point)
        /// </summary>
        [HttpGet("validate/{userId}")]
        public async Task<ActionResult<bool>> ValidateUser(int userId, CancellationToken ct)
        {
            _logger.LogInformation("Validating user: {UserId}", userId);

            // 🎯 DEMO: Uncomment to simulate slow response
            // await Task.Delay(5000, ct);

            // 🎯 DEMO: Uncomment to simulate failure
            // return StatusCode(500, "Service unavailable");

            var isValid = await _userService.ValidateUserAsync(userId, ct);
            return Ok(isValid);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetById(int id, CancellationToken ct)
        {
            var currentUserId = (int)HttpContext.Items["UserId"]!;
            _logger.LogInformation("User {CurrentUserId} requesting user {Id}", currentUserId, id);
            var user = await _userService.GetByIdAsync(id, ct);
            return Ok(ApiResponse<UserDto>.SuccessResponse(user));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserDto>>> Create(
            [FromBody] CreateUserRequest request,
            CancellationToken ct)
        {
            var user = await _userService.CreateAsync(request, ct);
            return CreatedAtAction(
                nameof(GetById),
                new { id = user.Id },
                ApiResponse<UserDto>.SuccessResponse(user, "User created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> Update(
            int id,
            [FromBody] UpdateUserRequest request,
            CancellationToken ct)
        {
            var user = await _userService.UpdateAsync(id, request, ct);
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User updated successfully"));
        }
    }
}