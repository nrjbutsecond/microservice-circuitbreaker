namespace Share.Resilience;

public class CircuitBreakerEvent
{
    public string ServiceName { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty; // StateChange, Success, Failure, Timeout
    public string? State { get; set; } // Closed, Open, HalfOpen
    public DateTime Timestamp { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Details { get; set; }
}

public class CircuitBreakerStatus
{
    public string ServiceName { get; set; } = string.Empty;
    public string State { get; set; } = "Closed";
    public int FailureCount { get; set; }
    public int SuccessCount { get; set; }
    public double SuccessRate { get; set; }
    public DateTime? LastFailureTime { get; set; }
    public DateTime? LastSuccessTime { get; set; }
    public DateTime? NextAttemptTime { get; set; }
    public TimeSpan? BreakDuration { get; set; }
}

public class CircuitBreakerMetrics
{
    public Dictionary<string, CircuitBreakerStatus> Services { get; set; } = new();
    public int TotalFailures { get; set; }
    public int TotalSuccesses { get; set; }
    public List<CircuitBreakerEvent> RecentEvents { get; set; } = new();
}
