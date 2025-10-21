using ReadingService.Core.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingService.Core.Application.Services
{
    public interface IReadingHistoryService
    {
        Task TrackReadingAsync(TrackReadingRequest request, CancellationToken ct = default);
        Task<List<ReadingHistoryDto>> GetUserHistoryAsync(int userId, CancellationToken ct = default);
        Task<UserReadingStatsDto> GetUserStatsAsync(int userId, CancellationToken ct = default);
    }
}
