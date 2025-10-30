using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Resilience
{
    /// <summary>
    /// WebSocket handler for streaming circuit breaker statistics in real-time
    /// </summary>
    public class CircuitBreakerWebSocketHandler
    {
        private readonly CircuitBreakerMonitor _monitor;
        private readonly TimeSpan _updateInterval;

        public CircuitBreakerWebSocketHandler(CircuitBreakerMonitor monitor, TimeSpan? updateInterval = null)
        {
            _monitor = monitor;
            _updateInterval = updateInterval ?? TimeSpan.FromSeconds(2);
        }

        /// <summary>
        /// Handle WebSocket connection and stream statistics
        /// </summary>
        public async Task HandleWebSocketAsync(WebSocket webSocket, CancellationToken cancellationToken = default)
        {
            try
            {
                // Send initial connection message
                await SendMessageAsync(webSocket, new
                {
                    Type = "connected",
                    Message = "Circuit Breaker Statistics Stream",
                    Timestamp = DateTime.UtcNow,
                    UpdateIntervalSeconds = _updateInterval.TotalSeconds
                }, cancellationToken);

                // Stream statistics while connection is open
                while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
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

                    await SendMessageAsync(webSocket, payload, cancellationToken);
                    await Task.Delay(_updateInterval, cancellationToken);
                }

                // Send disconnect message
                if (webSocket.State == WebSocketState.Open)
                {
                    await SendMessageAsync(webSocket, new
                    {
                        Type = "disconnecting",
                        Message = "Closing connection",
                        Timestamp = DateTime.UtcNow
                    }, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (WebSocketException)
            {
                // Connection closed by client
            }
            finally
            {
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                {
                    try
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Connection closed",
                            CancellationToken.None);
                    }
                    catch
                    {
                        // Ignore errors during close
                    }
                }
            }
        }

        /// <summary>
        /// Handle WebSocket connection with custom filter (e.g., specific service)
        /// </summary>
        public async Task HandleFilteredWebSocketAsync(
            WebSocket webSocket,
            string? serviceNameFilter = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await SendMessageAsync(webSocket, new
                {
                    Type = "connected",
                    Message = serviceNameFilter != null
                        ? $"Streaming stats for service: {serviceNameFilter}"
                        : "Circuit Breaker Statistics Stream",
                    ServiceFilter = serviceNameFilter,
                    Timestamp = DateTime.UtcNow,
                    UpdateIntervalSeconds = _updateInterval.TotalSeconds
                }, cancellationToken);

                while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    object payload;

                    if (!string.IsNullOrEmpty(serviceNameFilter))
                    {
                        // Stream specific service
                        var stats = _monitor.GetStats(serviceNameFilter);
                        payload = new
                        {
                            Type = "stats",
                            Timestamp = DateTime.UtcNow,
                            ServiceName = serviceNameFilter,
                            Stats = stats != null ? new
                            {
                                stats.ServiceName,
                                stats.State,
                                stats.TotalCalls,
                                stats.SuccessCount,
                                stats.FailureCount,
                                stats.SuccessRate,
                                stats.FailureRate,
                                stats.TimeoutCount,
                                stats.RejectedCount,
                                stats.OpenCount,
                                stats.HalfOpenCount,
                                stats.RecoveryCount,
                                stats.IsHealthy,
                                stats.LastSuccessTime,
                                stats.LastFailureTime,
                                stats.LastTimeoutTime,
                                stats.LastRejectionTime,
                                stats.LastOpenTime,
                                stats.LastException,
                                UptimeSeconds = (int)stats.Uptime.TotalSeconds
                            } : null,
                            Found = stats != null
                        };
                    }
                    else
                    {
                        // Stream all services
                        var allStats = _monitor.GetAllStats();
                        payload = new
                        {
                            Type = "stats",
                            Timestamp = DateTime.UtcNow,
                            TotalServices = allStats.Count,
                            Services = allStats.Values.Select(s => new
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
                                s.IsHealthy
                            }).ToArray()
                        };
                    }

                    await SendMessageAsync(webSocket, payload, cancellationToken);
                    await Task.Delay(_updateInterval, cancellationToken);
                }

                if (webSocket.State == WebSocketState.Open)
                {
                    await SendMessageAsync(webSocket, new
                    {
                        Type = "disconnecting",
                        Message = "Closing connection",
                        Timestamp = DateTime.UtcNow
                    }, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            catch (WebSocketException)
            {
                // Connection closed
            }
            finally
            {
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                {
                    try
                    {
                        await webSocket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Connection closed",
                            CancellationToken.None);
                    }
                    catch { }
                }
            }
        }

        private async Task SendMessageAsync(WebSocket webSocket, object data, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var buffer = Encoding.UTF8.GetBytes(json);
            await webSocket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken);
        }
    }
}
