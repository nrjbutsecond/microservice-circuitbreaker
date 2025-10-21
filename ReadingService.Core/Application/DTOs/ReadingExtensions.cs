using ReadingService.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingService.Core.Application.DTOs
{
    public static class ReadingExtensions
    {
        public static ReadingHistoryDto ToDto(this ReadingHistory history)
        {
            return new ReadingHistoryDto
            {
                Id = history.Id,
                UserId = history.UserId,
                ComicId = history.ComicId,
                ChapterId = history.ChapterId,
                ChapterNumber = history.ChapterNumber,
                ReadAt = history.ReadAt,
                ReadingDurationSeconds = history.ReadingDurationSeconds,
                Completed = history.Completed
            };
        }

        public static ReadingStatsDto ToDto(this ComicStats stats)
        {
            return new ReadingStatsDto
            {
                TotalReads = stats.TotalReads,
                UniqueReaders = stats.UniqueReaders,
                ActiveReaders24h = stats.ActiveReaders24h,
                AvgReadingTimeSeconds = stats.AvgReadingTimeSeconds
            };
        }
    }
}