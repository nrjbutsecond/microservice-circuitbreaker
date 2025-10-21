using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Resilience
{
    public class CircuitBreakerOptions
    {
        public int SamplingDurationSeconds { get; set; } = 10;
        public double FailureRatio { get; set; } = 0.5;
        public int MinimumThroughput { get; set; } = 3;
        public int BreakDurationSeconds { get; set; } = 30;
        public int TimeoutSeconds { get; set; } = 3;
        public int RetryCount { get; set; } = 2;
        public int RetryDelaySeconds { get; set; } = 1;
    }
}
