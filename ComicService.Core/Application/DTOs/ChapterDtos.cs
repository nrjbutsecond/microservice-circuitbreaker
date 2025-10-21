using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Core.Application.DTOs
{
    public class ChapterDto
    {
        public int Id { get; set; }
        public int ComicId { get; set; }
        public int ChapterNumber { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateChapterRequest
    {
        public int ChapterNumber { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; } = string.Empty;
    }

}
