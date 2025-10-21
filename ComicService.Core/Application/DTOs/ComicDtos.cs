using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Core.Application.DTOs
{
    public class ComicDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public int TotalChapters { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ComicDetailDto : ComicDto
    {
        // Data from Reading-Service (🔴 Circuit Breaker Point)
        public long TotalReads { get; set; }
        public int UniqueReaders { get; set; }
        public int ActiveReaders24h { get; set; }
        public bool IsTrending { get; set; }
    }

    public class ComicListItemDto : ComicDto
    {
        // Simplified stats for list view
        public long TotalReads { get; set; }
        public bool IsTrending { get; set; }
    }

    public class CreateComicRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
    }

    public class UpdateComicRequest
    {
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Status { get; set; }
    }
}