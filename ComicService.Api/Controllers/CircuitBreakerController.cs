using Common.Resilience;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace ComicService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CircuitBreakerController : ControllerBase
    {
        private readonly CircuitBreakerMonitor _monitor;
        private readonly CircuitBreakerWebSocketHandler _webSocketHandler;

        public CircuitBreakerController(CircuitBreakerMonitor monitor)
        {
            _monitor = monitor;
            _webSocketHandler = new CircuitBreakerWebSocketHandler(monitor);
        }

        /// <summary>
        /// Get circuit breaker statistics for all services
        /// </summary>
        [HttpGet("stats")]
        public IActionResult GetAllStats()
        {
            var stats = _monitor.GetAllStats();
            return Ok(new
            {
                Timestamp = DateTime.UtcNow,
                TotalServices = stats.Count,
                Services = stats.Select(s => new
                {
                    s.Value.ServiceName,
                    s.Value.State,
                    s.Value.TotalCalls,
                    s.Value.SuccessCount,
                    s.Value.FailureCount,
                    s.Value.SuccessRate,
                    s.Value.FailureRate,
                    s.Value.TimeoutCount,
                    s.Value.RejectedCount,
                    s.Value.OpenCount,
                    s.Value.HalfOpenCount,
                    s.Value.RecoveryCount,
                    s.Value.IsHealthy,
                    s.Value.LastSuccessTime,
                    s.Value.LastFailureTime,
                    s.Value.LastException,
                    UptimeSeconds = (int)s.Value.Uptime.TotalSeconds
                })
            });
        }

        /// <summary>
        /// Get circuit breaker statistics for a specific service
        /// </summary>
        [HttpGet("stats/{serviceName}")]
        public IActionResult GetServiceStats(string serviceName)
        {
            var stats = _monitor.GetStats(serviceName);
            if (stats == null)
            {
                return NotFound(new { Message = $"Service '{serviceName}' not found" });
            }

            return Ok(new
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
            });
        }

        /// <summary>
        /// Get circuit breaker info for a specific service (simplified view)
        /// </summary>
        [HttpGet("info/{serviceName}")]
        public IActionResult GetCircuitBreakerInfo(string serviceName)
        {
            var info = _monitor.GetCircuitBreakerInfo(serviceName);
            return Ok(info);
        }

        /// <summary>
        /// Get health summary of all services
        /// </summary>
        [HttpGet("health")]
        public IActionResult GetHealthSummary()
        {
            var stats = _monitor.GetAllStats();
            var healthyServices = stats.Values.Count(s => s.IsHealthy);
            var unhealthyServices = stats.Count - healthyServices;

            return Ok(new
            {
                Timestamp = DateTime.UtcNow,
                TotalServices = stats.Count,
                HealthyServices = healthyServices,
                UnhealthyServices = unhealthyServices,
                OverallHealth = stats.Count > 0 ? (double)healthyServices / stats.Count * 100 : 100,
                Services = stats.Select(s => new
                {
                    s.Value.ServiceName,
                    s.Value.State,
                    s.Value.IsHealthy,
                    s.Value.FailureRate,
                    Status = s.Value.State == CircuitBreakerState.Closed ? "Available" :
                             s.Value.State == CircuitBreakerState.Open ? "Down" : "Recovering"
                })
            });
        }

        /// <summary>
        /// Reset statistics for a specific service
        /// </summary>
        [HttpPost("reset/{serviceName}")]
        public IActionResult ResetServiceStats(string serviceName)
        {
            _monitor.ResetStats(serviceName);
            return Ok(new { Message = $"Statistics reset for service '{serviceName}'" });
        }

        /// <summary>
        /// Reset statistics for all services
        /// </summary>
        [HttpPost("reset")]
        public IActionResult ResetAllStats()
        {
            _monitor.ClearAllStats();
            return Ok(new { Message = "All statistics cleared" });
        }

        /// <summary>
        /// Get services grouped by state
        /// </summary>
        [HttpGet("by-state")]
        public IActionResult GetServicesByState()
        {
            var stats = _monitor.GetAllStats();
            var grouped = stats.Values
                .GroupBy(s => s.State)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Select(s => new
                    {
                        s.ServiceName,
                        s.FailureRate,
                        s.TotalCalls,
                        s.LastStateChange
                    }).ToList()
                );

            return Ok(new
            {
                Timestamp = DateTime.UtcNow,
                GroupedByState = grouped,
                Summary = new
                {
                    Closed = stats.Values.Count(s => s.State == CircuitBreakerState.Closed),
                    Open = stats.Values.Count(s => s.State == CircuitBreakerState.Open),
                    HalfOpen = stats.Values.Count(s => s.State == CircuitBreakerState.HalfOpen)
                }
            });
        }

        /// <summary>
        /// WebSocket endpoint for real-time circuit breaker statistics streaming
        /// Connect via: ws://localhost:PORT/api/circuitbreaker/ws
        /// </summary>
        [HttpGet("ws")]
        public async Task WebSocketEndpoint()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _webSocketHandler.HandleWebSocketAsync(webSocket, HttpContext.RequestAborted);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        /// <summary>
        /// WebSocket endpoint for streaming specific service statistics
        /// Connect via: ws://localhost:PORT/api/circuitbreaker/ws/{serviceName}
        /// </summary>
        [HttpGet("ws/{serviceName}")]
        public async Task WebSocketServiceEndpoint(string serviceName)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _webSocketHandler.HandleFilteredWebSocketAsync(webSocket, serviceName, HttpContext.RequestAborted);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }
    }
}
