using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Core.Application.DTOs;
using UserService.Core.Domain.Entities;
using UserService.Core.Domain.Interfaces;
using Shared.common;
using Microsoft.Extensions.Logging;
namespace UserService.Core.Application.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UserDto> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                throw new NotFoundException(nameof(User), id);
            }

            return user.ToDto();
        }

        public async Task<bool> ValidateUserAsync(int userId, CancellationToken ct = default)
        {
            // 🎯 DEMO: Có thể thêm delay để test circuit breaker
            // await Task.Delay(5000, ct); // Uncomment to simulate slow response

            var exists = await _userRepository.ExistsAsync(userId, ct);

            _logger.LogInformation("User validation for {UserId}: {Result}", userId, exists);

            return exists;
        }

        public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
        {
            // Validate
            var errors = new List<string>();

            if (await _userRepository.IsEmailTakenAsync(request.Email, ct))
            {
                errors.Add("Email is already taken");
            }

            if (await _userRepository.IsUsernameTakenAsync(request.Username, ct))
            {
                errors.Add("Username is already taken");
            }

            if (errors.Any())
            {
                throw new ValidationException(errors);
            }

            // Hash password (simplified - use BCrypt in production)
            var passwordHash = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(request.Password));

            var user = new User
            { 
                
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, ct);

            _logger.LogInformation("Created user: {UserId}", user.Id);

            return user.ToDto();
        }

        public async Task<UserDto> UpdateAsync(int id, UpdateUserRequest request, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                throw new NotFoundException(nameof(User), id);
            }

            if (request.FullName != null)
            {
                user.FullName = request.FullName;
            }

            if (request.AvatarUrl != null)
            {
                user.AvatarUrl = request.AvatarUrl;
            }

            await _userRepository.UpdateAsync(user, ct);

            _logger.LogInformation("Updated user: {UserId}", user.Id);

            return user.ToDto();
        }
    }
}
