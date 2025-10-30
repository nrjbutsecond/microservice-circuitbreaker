using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Resilience
{
    /// <summary>
    /// SignalR hub for streaming circuit breaker statistics in real-time
    /// </summary>
    public class CircuitBreakerHub : Hub
    {
        private readonly CircuitBreakerMonitor _monitor;
        private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(2);

        public CircuitBreakerHub(CircuitBreakerMonitor monitor)
        {
            _monitor = monitor;
        }

        /// <summary>
        /// Called when a client connects to the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await Clients.Caller.SendAsync("Connected", new
            {
                Message = "Circuit Breaker Statistics Stream",
                Timestamp = DateTime.UtcNow,
                UpdateIntervalSeconds = UpdateInterval.TotalSeconds,
                ConnectionId = Context.ConnectionId
            });

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects from the hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                Console.WriteLine($"Client disconnected with error: {exception.Message}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Subscribe to all circuit breaker statistics
        /// </summary>
        public async Task SubscribeToAllStats()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AllStats");
            
            // Send initial stats immediately
            await SendAllStats();
        }

        /// <summary>
        /// Subscribe to a specific service's statistics
        /// </summary>
        public async Task SubscribeToService(string serviceName)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Service_{serviceName}");
                
                // Send initial stats for the service
                await SendServiceStats(serviceName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SubscribeToService: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Unsubscribe from all statistics
        /// </summary>
        public async Task UnsubscribeFromAllStats()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AllStats");
        }

        /// <summary>
        /// Unsubscribe from a specific service
        /// </summary>
        public async Task UnsubscribeFromService(string serviceName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Service_{serviceName}");
        }

        /// <summary>
        /// Get current statistics for all services (on-demand)
        /// </summary>
        public async Task GetAllStats()
        {
            await SendAllStats();
        }

        /// <summary>
        /// Get current statistics for a specific service (on-demand)
        /// </summary>
        public async Task GetServiceStats(string serviceName)
        {
            await SendServiceStats(serviceName);
        }

        /// <summary>
        /// Send all statistics to the caller
        /// </summary>
        private async Task SendAllStats()
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

            await Clients.Caller.SendAsync("ReceiveStats", payload);
        }

        /// <summary>
        /// Send statistics for a specific service to the caller
        /// </summary>
        private async Task SendServiceStats(string serviceName)
        {
            var stats = _monitor.GetStats(serviceName);
            
            if (stats != null)
            {
                var payload = new
                {
                    Type = "serviceStats",
                    Timestamp = DateTime.UtcNow,
                    ServiceName = stats.ServiceName,
                    State = stats.State,
                    TotalCalls = stats.TotalCalls,
                    SuccessCount = stats.SuccessCount,
                    FailureCount = stats.FailureCount,
                    SuccessRate = stats.SuccessRate,
                    FailureRate = stats.FailureRate,
                    TimeoutCount = stats.TimeoutCount,
                    RejectedCount = stats.RejectedCount,
                    OpenCount = stats.OpenCount,
                    HalfOpenCount = stats.HalfOpenCount,
                    RecoveryCount = stats.RecoveryCount,
                    IsHealthy = stats.IsHealthy,
                    LastSuccessTime = stats.LastSuccessTime,
                    LastFailureTime = stats.LastFailureTime,
                    LastException = stats.LastException,
                    UptimeSeconds = (int)stats.Uptime.TotalSeconds
                };

                await Clients.Caller.SendAsync("ReceiveServiceStats", payload);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", new
                {
                    Message = $"Service '{serviceName}' not found",
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }
}
