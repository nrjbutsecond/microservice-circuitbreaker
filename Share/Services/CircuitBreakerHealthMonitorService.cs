using Common.Resilience;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Share.Services
{
    /// <summary>
    /// Background service that monitors circuit breaker health and logs alerts
    /// Add this to your API services to get automatic monitoring and logging
    /// 
    /// Usage in Program.cs:
    /// builder.Services.AddHostedService<CircuitBreakerHealthMonitorService>();
    /// </summary>
    public class CircuitBreakerHealthMonitorService : BackgroundService
    {
        private readonly ILogger<CircuitBreakerHealthMonitorService> _logger;
        private readonly CircuitBreakerMonitor _monitor;
        private readonly TimeSpan _checkInterval;

        public CircuitBreakerHealthMonitorService(
            ILogger<CircuitBreakerHealthMonitorService> logger,
            CircuitBreakerMonitor monitor,
            TimeSpan? checkInterval = null)
        {
            _logger = logger;
            _monitor = monitor;
            _checkInterval = checkInterval ?? TimeSpan.FromSeconds(30);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Circuit Breaker Health Monitor started. Check interval: {Interval} seconds",
                _checkInterval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await MonitorHealthAsync();
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when stopping
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Circuit Breaker Health Monitor");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

            _logger.LogInformation("Circuit Breaker Health Monitor stopped");
        }

        private Task MonitorHealthAsync()
        {
            var allStats = _monitor.GetAllStats();

            if (allStats.Count == 0)
            {
                // No services registered yet
                return Task.CompletedTask;
            }

            // Calculate overall health
            var healthyCount = allStats.Values.Count(s => s.IsHealthy);
            var totalCount = allStats.Count;
            var overallHealth = (double)healthyCount / totalCount * 100;

            // Count by state
            var closedCount = allStats.Values.Count(s => s.State == CircuitBreakerState.Closed);
            var openCount = allStats.Values.Count(s => s.State == CircuitBreakerState.Open);
            var halfOpenCount = allStats.Values.Count(s => s.State == CircuitBreakerState.HalfOpen);

            // Log overall status
            if (overallHealth < 100)
            {
                _logger.LogWarning(
                    "Circuit Breaker Health: {Health:F1}% | Healthy: {Healthy}/{Total} | States: Closed={Closed}, Open={Open}, HalfOpen={HalfOpen}",
                    overallHealth, healthyCount, totalCount, closedCount, openCount, halfOpenCount);
            }
            else
            {
                _logger.LogInformation(
                    "Circuit Breaker Health: {Health:F1}% | All services healthy ({Total}) | States: Closed={Closed}",
                    overallHealth, totalCount, closedCount);
            }

            // Alert on specific services
            foreach (var kvp in allStats)
            {
                var stats = kvp.Value;

                // Circuit is open - critical alert
                if (stats.State == CircuitBreakerState.Open)
                {
                    _logger.LogError(
                        "🔴 CIRCUIT OPEN: {ServiceName} | Failure Rate: {FailureRate:F2}% | Failures: {Failures}/{Total} | Rejected: {Rejected} | Last Error: {LastException}",
                        stats.ServiceName,
                        stats.FailureRate,
                        stats.FailureCount,
                        stats.TotalCalls,
                        stats.RejectedCount,
                        stats.LastException ?? "N/A");
                }
                // Circuit is half-open - recovery attempt
                else if (stats.State == CircuitBreakerState.HalfOpen)
                {
                    _logger.LogWarning(
                        "🟡 CIRCUIT HALF-OPEN: {ServiceName} | Attempting recovery | Failures: {Failures} | Recoveries: {Recoveries}",
                        stats.ServiceName,
                        stats.FailureCount,
                        stats.RecoveryCount);
                }
                // High failure rate warning
                else if (stats.FailureRate > 20 && stats.TotalCalls >= 10)
                {
                    _logger.LogWarning(
                        "⚠️ HIGH FAILURE RATE: {ServiceName} | Rate: {FailureRate:F2}% | Failures: {Failures}/{Total} | Timeouts: {Timeouts}",
                        stats.ServiceName,
                        stats.FailureRate,
                        stats.FailureCount,
                        stats.TotalCalls,
                        stats.TimeoutCount);
                }
                // Successful recovery
                else if (stats.RecoveryCount > 0 && stats.State == CircuitBreakerState.Closed && stats.IsHealthy)
                {
                    // Log recovery success only once per check cycle
                    if (stats.LastStateChange.HasValue &&
                        DateTime.UtcNow - stats.LastStateChange.Value < _checkInterval)
                    {
                        _logger.LogInformation(
                            "✅ CIRCUIT RECOVERED: {ServiceName} | Success Rate: {SuccessRate:F2}% | Total Recoveries: {Recoveries}",
                            stats.ServiceName,
                            stats.SuccessRate,
                            stats.RecoveryCount);
                    }
                }
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Circuit Breaker Health Monitor is stopping...");
            return base.StopAsync(cancellationToken);
        }
    }
}
