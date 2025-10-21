using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingService.Core.Domain.Entities
{
    public class ReadingHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ComicId { get; set; }
        public int ChapterId { get; set; }
        public int ChapterNumber { get; set; }
        public DateTime ReadAt { get; set; } = DateTime.UtcNow;
        public int ReadingDurationSeconds { get; set; }
        public bool Completed { get; set; }
    }
}
