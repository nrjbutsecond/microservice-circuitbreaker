using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingService.Core.Application.DTOs
{
    public class TrackReadingRequest
    {
        public int UserId { get; set; }
        public int ComicId { get; set; }
        public int ChapterId { get; set; }
        public int ChapterNumber { get; set; }
        public int ReadingDurationSeconds { get; set; }
        public bool Completed { get; set; }
    }

    public class ReadingHistoryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ComicId { get; set; }
        public int ChapterId { get; set; }
        public int ChapterNumber { get; set; }
        public DateTime ReadAt { get; set; }
        public int ReadingDurationSeconds { get; set; }
        public bool Completed { get; set; }

        // Enriched data (from other services)
        public string? ComicTitle { get; set; }
        public string? UserName { get; set; }
    }

    public class ReadingStatsDto
    {
        public long TotalReads { get; set; }
        public int UniqueReaders { get; set; }
        public int ActiveReaders24h { get; set; }
        public int AvgReadingTimeSeconds { get; set; }
    }

    public class UserReadingStatsDto
    {
        public int TotalComicsRead { get; set; }
        public int TotalChaptersRead { get; set; }
        public int TotalReadingTimeHours { get; set; }
        public DateTime? LastReadAt { get; set; }
    }
}
