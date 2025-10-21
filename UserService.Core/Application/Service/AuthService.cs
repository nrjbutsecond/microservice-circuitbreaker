using Microsoft.Extensions.Logging;
using Shared.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Core.Application.DTOs;
using UserService.Core.Domain.Interfaces;

namespace UserService.Core.Application.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, ct);
            if (user == null || !user.IsActive)
            {
                throw new ValidationException("Invalid email or password");
            }

            var passwordHash = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(request.Password));

            if (user.PasswordHash != passwordHash)
            {
                throw new ValidationException("Invalid email or password");
            }

            var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            _logger.LogInformation("User logged in: {UserId}", user.Id);

            return new LoginResponse
            {
                Token = token,
                User = user.ToDto()
            };
        }
    }
}