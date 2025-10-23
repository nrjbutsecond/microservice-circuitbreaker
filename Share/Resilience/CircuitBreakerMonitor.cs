using System.Collections.Concurrent;

namespace Share.Resilience;

public interface ICircuitBreakerMonitor
{
    void RecordEvent(CircuitBreakerEvent cbEvent);
    void UpdateStatus(string serviceName, string state, int failureCount, int successCount, DateTime? lastFailureTime, DateTime? lastSuccessTime, DateTime? nextAttemptTime, TimeSpan? breakDuration);
    void IncrementFailure(string serviceName);
    void IncrementSuccess(string serviceName);
    CircuitBreakerMetrics GetMetrics();
    CircuitBreakerStatus? GetServiceStatus(string serviceName);
    List<CircuitBreakerEvent> GetEvents(int limit = 100);
    List<CircuitBreakerEvent> GetServiceEvents(string serviceName, int limit = 100);
}

public class CircuitBreakerMonitor : ICircuitBreakerMonitor
{
    private readonly ConcurrentQueue<CircuitBreakerEvent> _events = new();
    private readonly ConcurrentDictionary<string, CircuitBreakerStatus> _serviceStatus = new();
    private const int MaxEvents = 1000; // Keep last 1000 events in memory

    public void RecordEvent(CircuitBreakerEvent cbEvent)
    {
        cbEvent.Timestamp = DateTime.UtcNow;
        _events.Enqueue(cbEvent);

        // Limit the queue size
        while (_events.Count > MaxEvents)
        {
            _events.TryDequeue(out _);
        }
    }

    public void UpdateStatus(string serviceName, string state, int failureCount, int successCount,
        DateTime? lastFailureTime, DateTime? lastSuccessTime, DateTime? nextAttemptTime, TimeSpan? breakDuration)
    {
        var status = _serviceStatus.GetOrAdd(serviceName, _ => new CircuitBreakerStatus { ServiceName = serviceName });

        status.State = state;
        status.FailureCount = failureCount;
        status.SuccessCount = successCount;
        status.LastFailureTime = lastFailureTime;
        status.LastSuccessTime = lastSuccessTime;
        status.NextAttemptTime = nextAttemptTime;
        status.BreakDuration = breakDuration;

        // Calculate success rate
        int total = failureCount + successCount;
        status.SuccessRate = total > 0 ? (double)successCount / total : 1.0;
    }

    public void IncrementFailure(string serviceName)
    {
        var status = _serviceStatus.GetOrAdd(serviceName, _ => new CircuitBreakerStatus { ServiceName = serviceName });
        status.FailureCount++;
        status.LastFailureTime = DateTime.UtcNow;

        // Recalculate success rate
        int total = status.FailureCount + status.SuccessCount;
        status.SuccessRate = total > 0 ? (double)status.SuccessCount / total : 1.0;
    }

    public void IncrementSuccess(string serviceName)
    {
        var status = _serviceStatus.GetOrAdd(serviceName, _ => new CircuitBreakerStatus { ServiceName = serviceName });
        status.SuccessCount++;
        status.LastSuccessTime = DateTime.UtcNow;

        // Recalculate success rate
        int total = status.FailureCount + status.SuccessCount;
        status.SuccessRate = total > 0 ? (double)status.SuccessCount / total : 1.0;
    }

    public CircuitBreakerMetrics GetMetrics()
    {
        var events = _events.ToList();
        var metrics = new CircuitBreakerMetrics
        {
            Services = new Dictionary<string, CircuitBreakerStatus>(_serviceStatus),
            RecentEvents = events.OrderByDescending(e => e.Timestamp).Take(100).ToList(),
            TotalFailures = events.Count(e => e.EventType == "Failure"),
            TotalSuccesses = events.Count(e => e.EventType == "Success")
        };

        return metrics;
    }

    public CircuitBreakerStatus? GetServiceStatus(string serviceName)
    {
        _serviceStatus.TryGetValue(serviceName, out var status);
        return status;
    }

    public List<CircuitBreakerEvent> GetEvents(int limit = 100)
    {
        return _events
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToList();
    }

    public List<CircuitBreakerEvent> GetServiceEvents(string serviceName, int limit = 100)
    {
        return _events
            .Where(e => e.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(e => e.Timestamp)
            .Take(limit)
            .ToList();
    }
}
