using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComicService.Core.Application.DTOs
{
    public class ReadingStatsDto
    {
        public long TotalReads { get; set; }
        public int UniqueReaders { get; set; }
        public int ActiveReaders24h { get; set; }
        public int AvgReadingTimeSeconds { get; set; }
    }
}