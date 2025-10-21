using ReadingService.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingService.Core.Domain.Interfaces
{
    public interface IReadingHistoryRepository
    {
        Task<List<ReadingHistory>> GetByUserIdAsync(int userId, CancellationToken ct = default);
        Task<List<ReadingHistory>> GetByComicIdAsync(int comicId, CancellationToken ct = default);
        Task<bool> HasUserReadComicAsync(int userId, int comicId, CancellationToken ct = default);
        Task<ReadingHistory> AddAsync(ReadingHistory history, CancellationToken ct = default);
    }
}
