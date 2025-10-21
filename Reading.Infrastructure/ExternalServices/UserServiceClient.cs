using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using ReadingService.Core.Application.DTOs;
using ReadingService.Core.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Reading.Infrastructure.ExternalServices
{
    public class UserServiceClient : IUserServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserServiceClient> _logger;

        public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> ValidateUserAsync(int userId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("🔹 Calling User-Service: Validate user {UserId}", userId);

                // Circuit breaker is handled by Polly at HttpClient level
                var response = await _httpClient.GetAsync($"api/users/validate/{userId}", ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "⚠️ User-Service returned {StatusCode} for user {UserId}",
                        response.StatusCode, userId);
                    return false;
                }

                var result = await response.Content.ReadFromJsonAsync<bool>(cancellationToken: ct);

                _logger.LogInformation("✅ User validation successful: {UserId} = {Result}", userId, result);

                return result;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex,
                    "🔴 CIRCUIT BREAKER OPEN: User-Service is unavailable for user {UserId}",
                    userId);
                throw; // Re-throw to be handled by service layer
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "❌ HTTP Error calling User-Service for user {UserId}",
                    userId);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex,
                    "⏱️ TIMEOUT calling User-Service for user {UserId}",
                    userId);
                throw;
            }
        }

        public async Task<UserDto?> GetUserAsync(int userId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("🔹 Calling User-Service: Get user {UserId}", userId);

                var response = await _httpClient.GetAsync($"api/users/{userId}", ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "⚠️ User-Service returned {StatusCode} for user {UserId}",
                        response.StatusCode, userId);
                    return null;
                }

                // Response is wrapped in ApiResponse<UserDto>
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>(cancellationToken: ct);

                _logger.LogInformation("✅ User fetched successfully: {UserId}", userId);

                return apiResponse?.Data;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex,
                    "🔴 CIRCUIT BREAKER OPEN: User-Service is unavailable for user {UserId}",
                    userId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error calling User-Service for user {UserId}", userId);
                throw;
            }
        }
    }
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public T? Data { get; set; }
        }
    
}
