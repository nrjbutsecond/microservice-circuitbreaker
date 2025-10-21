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
    /// <summary>
    /// 🔴 CIRCUIT BREAKER: Calls to Comic-Service
    /// Polly circuit breaker is configured in DI
    /// </summary>
    public class ComicServiceClient : IComicServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ComicServiceClient> _logger;

        public ComicServiceClient(HttpClient httpClient, ILogger<ComicServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ComicDto?> GetComicAsync(int comicId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("🔹 Calling Comic-Service: Get comic {ComicId}", comicId);

                var response = await _httpClient.GetAsync($"api/comics/{comicId}", ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "⚠️ Comic-Service returned {StatusCode} for comic {ComicId}",
                        response.StatusCode, comicId);
                    return null;
                }

                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ComicDto>>(cancellationToken: ct);

                _logger.LogInformation("✅ Comic fetched successfully: {ComicId}", comicId);

                return apiResponse?.Data;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex,
                    "🔴 CIRCUIT BREAKER OPEN: Comic-Service is unavailable for comic {ComicId}",
                    comicId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error calling Comic-Service for comic {ComicId}", comicId);
                throw;
            }
        }

        public async Task<Dictionary<int, ComicDto>> GetBatchComicsAsync(
            List<int> comicIds,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation(
                    "🔹 Calling Comic-Service: Get batch {Count} comics",
                    comicIds.Count);

                var queryString = string.Join("&", comicIds.Select(id => $"ids={id}"));
                var response = await _httpClient.GetAsync($"api/comics/batch?{queryString}", ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "⚠️ Comic-Service returned {StatusCode} for batch request",
                        response.StatusCode);
                    return new Dictionary<int, ComicDto>();
                }

                var apiResponse = await response.Content
                    .ReadFromJsonAsync<ApiResponse<List<ComicDto>>>(cancellationToken: ct);

                if (apiResponse?.Data == null)
                {
                    return new Dictionary<int, ComicDto>();
                }

                var result = apiResponse.Data.ToDictionary(c => c.Id);

                _logger.LogInformation("✅ Batch comics fetched: {Count} results", result.Count);

                return result;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex,
                    "🔴 CIRCUIT BREAKER OPEN: Comic-Service is unavailable for batch request");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error calling Comic-Service for batch request");
                throw;
            }
        }
    }
}