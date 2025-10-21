using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadingService.Core.Domain.Entities
{
    public class ComicStats
    {
        public int ComicId { get; set; }
        public long TotalReads { get; set; }
        public int UniqueReaders { get; set; }
        public int ActiveReaders24h { get; set; }
        public int AvgReadingTimeSeconds { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
