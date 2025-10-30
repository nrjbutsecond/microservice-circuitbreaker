using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Resilience
{
    /// <summary>
    /// Background service that periodically broadcasts circuit breaker statistics to all connected clients
    /// </summary>
    public class CircuitBreakerBroadcastService : BackgroundService
    {
        private readonly IHubContext<CircuitBreakerHub> _hubContext;
        private readonly CircuitBreakerMonitor _monitor;
        private readonly TimeSpan _updateInterval;

        public CircuitBreakerBroadcastService(
            IHubContext<CircuitBreakerHub> hubContext,
            CircuitBreakerMonitor monitor,
            TimeSpan? updateInterval = null)
        {
            _hubContext = hubContext;
            _monitor = monitor;
            _updateInterval = updateInterval ?? TimeSpan.FromSeconds(2);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await BroadcastAllStats(stoppingToken);
                    await Task.Delay(_updateInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when stopping
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error broadcasting stats: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task BroadcastAllStats(CancellationToken cancellationToken)
        {
            var stats = _monitor.GetAllStats();
            
            var payload = new
            {
                Type = "stats",
                Timestamp = DateTime.UtcNow,
                TotalServices = stats.Count,
                Services = stats.Values.Select(s => new
                {
                    s.ServiceName,
                    s.State,
                    s.TotalCalls,
                    s.SuccessCount,
                    s.FailureCount,
                    s.SuccessRate,
                    s.FailureRate,
                    s.TimeoutCount,
                    s.RejectedCount,
                    s.OpenCount,
                    s.HalfOpenCount,
                    s.RecoveryCount,
                    s.IsHealthy,
                    s.LastSuccessTime,
                    s.LastFailureTime,
                    s.LastException,
                    UptimeSeconds = (int)s.Uptime.TotalSeconds
                }).ToArray()
            };

            // Broadcast to all clients subscribed to "AllStats" group
            await _hubContext.Clients.Group("AllStats").SendAsync("ReceiveStats", payload, cancellationToken);
            
            // Also broadcast individual service stats to their respective groups
            foreach (var stat in stats.Values)
            {
                var servicePayload = new
                {
                    Type = "serviceStats",
                    Timestamp = DateTime.UtcNow,
                    ServiceName = stat.ServiceName,
                    State = stat.State,
                    TotalCalls = stat.TotalCalls,
                    SuccessCount = stat.SuccessCount,
                    FailureCount = stat.FailureCount,
                    SuccessRate = stat.SuccessRate,
                    FailureRate = stat.FailureRate,
                    TimeoutCount = stat.TimeoutCount,
                    RejectedCount = stat.RejectedCount,
                    OpenCount = stat.OpenCount,
                    HalfOpenCount = stat.HalfOpenCount,
                    RecoveryCount = stat.RecoveryCount,
                    IsHealthy = stat.IsHealthy,
                    LastSuccessTime = stat.LastSuccessTime,
                    LastFailureTime = stat.LastFailureTime,
                    LastException = stat.LastException,
                    UptimeSeconds = (int)stat.Uptime.TotalSeconds
                };

                await _hubContext.Clients.Group($"Service_{stat.ServiceName}")
                    .SendAsync("ReceiveServiceStats", servicePayload, cancellationToken);
            }
        }
    }
}
