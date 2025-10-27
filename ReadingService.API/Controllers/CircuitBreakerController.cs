using Common.Resilience;
using Microsoft.AspNetCore.Mvc;

namespace ReadingService.API.Controllers
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

            return Ok(stats);
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
        /// WebSocket endpoint for real-time circuit breaker statistics streaming
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
