using Microsoft.EntityFrameworkCore;
using ReadingService.Core.Domain.Entities;
using ReadingService.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reading.Infrastructure.Persistence.Repositories
{
    public class ReadingHistoryRepository : IReadingHistoryRepository
    {
        private readonly ReadingDbContext _context;

        public ReadingHistoryRepository(ReadingDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReadingHistory>> GetByUserIdAsync(int userId, CancellationToken ct = default)
        {
            return await _context.ReadingHistory
                .AsNoTracking()
                .Where(h => h.UserId == userId)
                .OrderByDescending(h => h.ReadAt)
                .ToListAsync(ct);
        }

        public async Task<List<ReadingHistory>> GetByComicIdAsync(int comicId, CancellationToken ct = default)
        {
            return await _context.ReadingHistory
                .AsNoTracking()
                .Where(h => h.ComicId == comicId)
                .OrderByDescending(h => h.ReadAt)
                .ToListAsync(ct);
        }

        public async Task<bool> HasUserReadComicAsync(int userId, int comicId, CancellationToken ct = default)
        {
            return await _context.ReadingHistory
                .AnyAsync(h => h.UserId == userId && h.ComicId == comicId, ct);
        }

        public async Task<ReadingHistory> AddAsync(ReadingHistory history, CancellationToken ct = default)
        {
            await _context.ReadingHistory.AddAsync(history, ct);
            await _context.SaveChangesAsync(ct);
            return history;
        }
    }
}