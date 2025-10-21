using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Resilience
{
    public enum CircuitBreakerState
    {
        Closed,
        Open,
        HalfOpen
    }

    public class CircuitBreakerInfo
    {
        public string ServiceName { get; set; } = string.Empty;
        public CircuitBreakerState State { get; set; }
        public int FailureCount { get; set; }
        public double SuccessRate { get; set; }
        public DateTime? LastFailureTime { get; set; }
        public DateTime? NextAttemptTime { get; set; }
    }
}