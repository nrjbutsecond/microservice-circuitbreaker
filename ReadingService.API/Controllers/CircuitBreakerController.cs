using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Share.Resilience;

namespace ReadingService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CircuitBreakerController : ControllerBase
{
    private readonly ICircuitBreakerMonitor _monitor;
    private readonly ILogger<CircuitBreakerController> _logger;

    public CircuitBreakerController(ICircuitBreakerMonitor monitor, ILogger<CircuitBreakerController> logger)
    {
        _monitor = monitor;
        _logger = logger;
    }

    /// <summary>
    /// Get current status of all circuit breakers
    /// </summary>
    [HttpGet("status")]
    public ActionResult<ApiResponse<CircuitBreakerMetrics>> GetStatus()
    {
        _logger.LogInformation("📊 Getting circuit breaker status");

        var metrics = _monitor.GetMetrics();
        return Ok(ApiResponse<CircuitBreakerMetrics>.SuccessResponse(
            metrics,
            "Circuit breaker status retrieved successfully"
        ));
    }

    /// <summary>
    /// Get recent circuit breaker events/logs
    /// </summary>
    [HttpGet("logs")]
    public ActionResult<ApiResponse<List<CircuitBreakerEvent>>> GetLogs([FromQuery] int limit = 100)
    {
        _logger.LogInformation("📋 Getting circuit breaker logs (limit: {Limit})", limit);

        var events = _monitor.GetEvents(limit);
        return Ok(ApiResponse<List<CircuitBreakerEvent>>.SuccessResponse(
            events,
            $"Retrieved {events.Count} circuit breaker events"
        ));
    }

    /// <summary>
    /// Get circuit breaker events for a specific service
    /// </summary>
    [HttpGet("logs/{serviceName}")]
    public ActionResult<ApiResponse<List<CircuitBreakerEvent>>> GetServiceLogs(
        string serviceName,
        [FromQuery] int limit = 100)
    {
        _logger.LogInformation("📋 Getting circuit breaker logs for {ServiceName} (limit: {Limit})",
            serviceName, limit);

        var events = _monitor.GetServiceEvents(serviceName, limit);
        return Ok(ApiResponse<List<CircuitBreakerEvent>>.SuccessResponse(
            events,
            $"Retrieved {events.Count} events for {serviceName}"
        ));
    }

    /// <summary>
    /// Get status of a specific circuit breaker
    /// </summary>
    [HttpGet("status/{serviceName}")]
    public ActionResult<ApiResponse<CircuitBreakerStatus>> GetServiceStatus(string serviceName)
    {
        _logger.LogInformation("🔍 Getting circuit breaker status for {ServiceName}", serviceName);

        var status = _monitor.GetServiceStatus(serviceName);
        if (status == null)
        {
            return NotFound(ApiResponse<CircuitBreakerStatus>.ErrorResponse(
                $"No circuit breaker found for service: {serviceName}"
            ));
        }

        return Ok(ApiResponse<CircuitBreakerStatus>.SuccessResponse(
            status,
            $"Circuit breaker status for {serviceName}"
        ));
    }
}
