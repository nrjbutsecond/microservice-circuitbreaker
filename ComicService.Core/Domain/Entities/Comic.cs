using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Core.Domain.Entities
{
    public class Comic
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Author { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public int TotalChapters { get; set; }
        public ComicStatus Status { get; set; } = ComicStatus.Ongoing;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
    }

    public enum ComicStatus
    {
        Ongoing,
        Completed,
        Hiatus
    }
}

