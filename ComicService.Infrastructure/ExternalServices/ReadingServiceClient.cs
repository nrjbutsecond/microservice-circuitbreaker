using ComicService.Core.Application.DTOs;
using ComicService.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Infrastructure.ExternalServices
{
    public class ReadingServiceClient : IReadingServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ReadingServiceClient> _logger;

        public ReadingServiceClient(HttpClient httpClient, ILogger<ReadingServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ReadingStatsDto?> GetComicStatsAsync(int comicId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("🔹 Calling Reading-Service: Get stats for comic {ComicId}", comicId);

                var response = await _httpClient.GetAsync($"api/stats/comic/{comicId}", ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "⚠️ Reading-Service returned {StatusCode} for comic {ComicId}",
                        response.StatusCode, comicId);
                    return null;
                }

                var stats = await response.Content.ReadFromJsonAsync<ReadingStatsDto>(cancellationToken: ct);

                _logger.LogInformation("✅ Stats fetched successfully for comic {ComicId}", comicId);

                return stats;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex,
                    "🔴 CIRCUIT BREAKER OPEN: Reading-Service is unavailable for comic {ComicId}",
                    comicId);
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "❌ HTTP Error calling Reading-Service for comic {ComicId}",
                    comicId);
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex,
                    "⏱️ TIMEOUT calling Reading-Service for comic {ComicId}",
                    comicId);
                throw;
            }
        }

        public async Task<Dictionary<int, ReadingStatsDto>> GetBatchStatsAsync(
            List<int> comicIds,
            CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation(
                    "🔹 Calling Reading-Service: Get batch stats for {Count} comics",
                    comicIds.Count);

                var queryString = string.Join("&", comicIds.Select(id => $"ids={id}"));
                var response = await _httpClient.GetAsync($"api/stats/comics/batch?{queryString}", ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "⚠️ Reading-Service returned {StatusCode} for batch request",
                        response.StatusCode);
                    return new Dictionary<int, ReadingStatsDto>();
                }

                var result = await response.Content
                    .ReadFromJsonAsync<Dictionary<int, ReadingStatsDto>>(cancellationToken: ct);

                _logger.LogInformation("✅ Batch stats fetched: {Count} results", result?.Count ?? 0);

                return result ?? new Dictionary<int, ReadingStatsDto>();
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogError(ex,
                    "🔴 CIRCUIT BREAKER OPEN: Reading-Service is unavailable for batch request");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error calling Reading-Service for batch request");
                throw;
            }
        }
    }
}