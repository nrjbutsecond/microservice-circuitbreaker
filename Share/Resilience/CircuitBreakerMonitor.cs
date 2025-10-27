using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Common.Resilience
{
    /// <summary>
    /// Service to track circuit breaker statistics across all services.
    /// Register as singleton: services.AddSingleton<CircuitBreakerMonitor>();
    /// </summary>
    public class CircuitBreakerMonitor
    {
        private readonly ConcurrentDictionary<string, CircuitBreakerStats> _stats;
        private readonly object _lock = new object();

        public CircuitBreakerMonitor()
        {
            _stats = new ConcurrentDictionary<string, CircuitBreakerStats>();
        }

        /// <summary>
        /// Register a new service to monitor
        /// </summary>
        public void RegisterService(string serviceName)
        {
            _stats.TryAdd(serviceName, new CircuitBreakerStats
            {
                ServiceName = serviceName,
                State = CircuitBreakerState.Closed,
                RegisteredAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Record a successful call
        /// </summary>
        public void RecordSuccess(string serviceName)
        {
            var stats = GetOrCreateStats(serviceName);
            lock (stats.Lock)
            {
                stats.TotalCalls++;
                stats.SuccessCount++;
                stats.LastSuccessTime = DateTime.UtcNow;
                UpdateFailureRate(stats);
            }
        }

        /// <summary>
        /// Record a failed call
        /// </summary>
        public void RecordFailure(string serviceName, Exception? exception = null)
        {
            var stats = GetOrCreateStats(serviceName);
            lock (stats.Lock)
            {
                stats.TotalCalls++;
                stats.FailureCount++;
                stats.LastFailureTime = DateTime.UtcNow;
                stats.LastException = exception?.Message;
                UpdateFailureRate(stats);
            }
        }

        /// <summary>
        /// Update circuit breaker state
        /// </summary>
        public void UpdateState(string serviceName, CircuitBreakerState newState)
        {
            var stats = GetOrCreateStats(serviceName);
            lock (stats.Lock)
            {
                var previousState = stats.State;
                stats.State = newState;
                stats.LastStateChange = DateTime.UtcNow;

                // Track state transitions
                switch (newState)
                {
                    case CircuitBreakerState.Open:
                        stats.OpenCount++;
                        stats.LastOpenTime = DateTime.UtcNow;
                        break;
                    case CircuitBreakerState.HalfOpen:
                        stats.HalfOpenCount++;
                        break;
                    case CircuitBreakerState.Closed when previousState == CircuitBreakerState.HalfOpen:
                        // Successfully recovered
                        stats.RecoveryCount++;
                        break;
                }
            }
        }

        /// <summary>
        /// Record a timeout
        /// </summary>
        public void RecordTimeout(string serviceName)
        {
            var stats = GetOrCreateStats(serviceName);
            lock (stats.Lock)
            {
                stats.TimeoutCount++;
                stats.LastTimeoutTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Record a rejected call (when circuit is open)
        /// </summary>
        public void RecordRejection(string serviceName)
        {
            var stats = GetOrCreateStats(serviceName);
            lock (stats.Lock)
            {
                stats.RejectedCount++;
                stats.LastRejectionTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Get statistics for a specific service
        /// </summary>
        public CircuitBreakerStats? GetStats(string serviceName)
        {
            return _stats.TryGetValue(serviceName, out var stats) ? stats.Clone() : null;
        }

        /// <summary>
        /// Get statistics for all services
        /// </summary>
        public IReadOnlyDictionary<string, CircuitBreakerStats> GetAllStats()
        {
            return _stats.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Clone()
            );
        }

        /// <summary>
        /// Get current circuit breaker info for a service
        /// </summary>
        public CircuitBreakerInfo GetCircuitBreakerInfo(string serviceName)
        {
            var stats = GetStats(serviceName);
            if (stats == null)
            {
                return new CircuitBreakerInfo
                {
                    ServiceName = serviceName,
                    State = CircuitBreakerState.Closed
                };
            }

            return new CircuitBreakerInfo
            {
                ServiceName = serviceName,
                State = stats.State,
                FailureCount = stats.FailureCount,
                SuccessRate = stats.SuccessRate,
                LastFailureTime = stats.LastFailureTime,
                NextAttemptTime = stats.State == CircuitBreakerState.Open ? stats.LastOpenTime?.AddSeconds(30) : null
            };
        }

        /// <summary>
        /// Reset statistics for a specific service
        /// </summary>
        public void ResetStats(string serviceName)
        {
            if (_stats.TryGetValue(serviceName, out var stats))
            {
                lock (stats.Lock)
                {
                    var currentState = stats.State;
                    var registeredAt = stats.RegisteredAt;
                    
                    _stats[serviceName] = new CircuitBreakerStats
                    {
                        ServiceName = serviceName,
                        State = currentState,
                        RegisteredAt = registeredAt
                    };
                }
            }
        }

        /// <summary>
        /// Clear all statistics
        /// </summary>
        public void ClearAllStats()
        {
            _stats.Clear();
        }

        private CircuitBreakerStats GetOrCreateStats(string serviceName)
        {
            return _stats.GetOrAdd(serviceName, name => new CircuitBreakerStats
            {
                ServiceName = name,
                State = CircuitBreakerState.Closed,
                RegisteredAt = DateTime.UtcNow
            });
        }

        private void UpdateFailureRate(CircuitBreakerStats stats)
        {
            if (stats.TotalCalls > 0)
            {
                stats.SuccessRate = (double)stats.SuccessCount / stats.TotalCalls * 100;
                stats.FailureRate = (double)stats.FailureCount / stats.TotalCalls * 100;
            }
        }
    }

    /// <summary>
    /// Detailed statistics for a circuit breaker
    /// </summary>
    public class CircuitBreakerStats
    {
        internal readonly object Lock = new object();

        public string ServiceName { get; set; } = string.Empty;
        public CircuitBreakerState State { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? LastStateChange { get; set; }

        // Call Statistics
        public long TotalCalls { get; set; }
        public long SuccessCount { get; set; }
        public long FailureCount { get; set; }
        public long TimeoutCount { get; set; }
        public long RejectedCount { get; set; }

        // Rates
        public double SuccessRate { get; set; }
        public double FailureRate { get; set; }

        // State Transitions
        public int OpenCount { get; set; }
        public int HalfOpenCount { get; set; }
        public int RecoveryCount { get; set; }

        // Timestamps
        public DateTime? LastSuccessTime { get; set; }
        public DateTime? LastFailureTime { get; set; }
        public DateTime? LastTimeoutTime { get; set; }
        public DateTime? LastRejectionTime { get; set; }
        public DateTime? LastOpenTime { get; set; }

        // Error Information
        public string? LastException { get; set; }

        // Health Metrics
        public bool IsHealthy => State == CircuitBreakerState.Closed && FailureRate < 10;
        public TimeSpan Uptime => DateTime.UtcNow - RegisteredAt;

        public CircuitBreakerStats Clone()
        {
            lock (Lock)
            {
                return new CircuitBreakerStats
                {
                    ServiceName = ServiceName,
                    State = State,
                    RegisteredAt = RegisteredAt,
                    LastStateChange = LastStateChange,
                    TotalCalls = TotalCalls,
                    SuccessCount = SuccessCount,
                    FailureCount = FailureCount,
                    TimeoutCount = TimeoutCount,
                    RejectedCount = RejectedCount,
                    SuccessRate = SuccessRate,
                    FailureRate = FailureRate,
                    OpenCount = OpenCount,
                    HalfOpenCount = HalfOpenCount,
                    RecoveryCount = RecoveryCount,
                    LastSuccessTime = LastSuccessTime,
                    LastFailureTime = LastFailureTime,
                    LastTimeoutTime = LastTimeoutTime,
                    LastRejectionTime = LastRejectionTime,
                    LastOpenTime = LastOpenTime,
                    LastException = LastException
                };
            }
        }

        public override string ToString()
        {
            return $"Service: {ServiceName}, State: {State}, Success Rate: {SuccessRate:F2}%, " +
                   $"Total Calls: {TotalCalls}, Failures: {FailureCount}, Rejected: {RejectedCount}";
        }
    }
}
