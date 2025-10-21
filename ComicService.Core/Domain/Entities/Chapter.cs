using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Core.Domain.Entities
{
    public class Chapter
    {
        public int Id { get; set; }
        public int ComicId { get; set; }
        public int ChapterNumber { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Comic Comic { get; set; } = null!;
    }
}