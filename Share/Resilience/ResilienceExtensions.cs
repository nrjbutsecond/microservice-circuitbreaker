using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Resilience
{
    public static class ResilienceExtensions
    {
        public static IHttpClientBuilder AddCustomResilience(
            this IHttpClientBuilder builder,
            CircuitBreakerOptions? options = null)
        {
            options ??= new CircuitBreakerOptions();

            builder.AddStandardResilienceHandler(config =>
            {
                // Circuit Breaker
                config.CircuitBreaker.SamplingDuration =
                    TimeSpan.FromSeconds(options.SamplingDurationSeconds);
                config.CircuitBreaker.FailureRatio = options.FailureRatio;
                config.CircuitBreaker.MinimumThroughput = options.MinimumThroughput;
                config.CircuitBreaker.BreakDuration =
                    TimeSpan.FromSeconds(options.BreakDurationSeconds);

                // Timeout
                config.AttemptTimeout.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                config.TotalRequestTimeout.Timeout =
                    TimeSpan.FromSeconds(options.TimeoutSeconds * 3);

                // Retry
                config.Retry.MaxRetryAttempts = options.RetryCount;
                config.Retry.Delay = TimeSpan.FromSeconds(options.RetryDelaySeconds);
                config.Retry.BackoffType = DelayBackoffType.Exponential;
            });

            return builder;
        }

        public static IHttpClientBuilder AddCustomResilienceWithLogging(
            this IHttpClientBuilder builder,
            string serviceName,
            ICircuitBreakerMonitor? monitor = null,
            CircuitBreakerOptions? options = null)
        {
            options ??= new CircuitBreakerOptions();
            
            // Store reference to monitor to be used in event handlers
            CircuitBreakerMonitor? monitorRef = null;
            
            // Configure resilience with event handlers
            builder.AddStandardResilienceHandler(config =>
            {
                // Circuit Breaker Configuration
                config.CircuitBreaker.SamplingDuration =
                    TimeSpan.FromSeconds(options.SamplingDurationSeconds);
                config.CircuitBreaker.FailureRatio = options.FailureRatio;
                config.CircuitBreaker.MinimumThroughput = options.MinimumThroughput;
                config.CircuitBreaker.BreakDuration =
                    TimeSpan.FromSeconds(options.BreakDurationSeconds);

                // ✅ Hook into circuit breaker state change events
                config.CircuitBreaker.OnOpened = args =>
                {
                    if (monitorRef != null)
                    {
                        monitorRef.UpdateState(serviceName, CircuitBreakerState.Open);
                        Console.WriteLine($"🔴 Circuit Breaker OPENED for {serviceName} - Break duration: {args.BreakDuration}");
                    }
                    return ValueTask.CompletedTask;
                };

                config.CircuitBreaker.OnClosed = args =>
                {
                    if (monitorRef != null)
                    {
                        monitorRef.UpdateState(serviceName, CircuitBreakerState.Closed);
                        Console.WriteLine($"🟢 Circuit Breaker CLOSED for {serviceName} - Service recovered");
                    }
                    return ValueTask.CompletedTask;
                };

                config.CircuitBreaker.OnHalfOpened = args =>
                {
                    if (monitorRef != null)
                    {
                        monitorRef.UpdateState(serviceName, CircuitBreakerState.HalfOpen);
                        Console.WriteLine($"🟡 Circuit Breaker HALF-OPEN for {serviceName} - Testing if service recovered");
                    }
                    return ValueTask.CompletedTask;
                };

                // Timeout Configuration
                config.AttemptTimeout.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                config.AttemptTimeout.OnTimeout = args =>
                {
                    if (monitorRef != null)
                    {
                        monitorRef.RecordTimeout(serviceName);
                        Console.WriteLine($"⏱️ TIMEOUT for {serviceName} after {args.Timeout}");
                    }
                    return ValueTask.CompletedTask;
                };

                config.TotalRequestTimeout.Timeout =
                    TimeSpan.FromSeconds(options.TimeoutSeconds * 3);

                // Retry Configuration
                config.Retry.MaxRetryAttempts = options.RetryCount;
                config.Retry.Delay = TimeSpan.FromSeconds(options.RetryDelaySeconds);
                config.Retry.BackoffType = DelayBackoffType.Exponential;

                config.Retry.OnRetry = args =>
                {
                    Console.WriteLine($"🔄 RETRY {args.AttemptNumber}/{options.RetryCount} for {serviceName}");
                    return ValueTask.CompletedTask;
                };
            });

            // Add monitoring handler to track successes and failures
            builder.AddHttpMessageHandler(sp =>
            {
                var monitor = sp.GetRequiredService<CircuitBreakerMonitor>();
                monitorRef = monitor; // Capture reference for event handlers
                monitor.RegisterService(serviceName);
                return new CircuitBreakerMonitoringHandler(serviceName, monitor);
            });

            return builder;
        }
    }

    /// <summary>
    /// HTTP handler that monitors circuit breaker events
    /// </summary>
    internal class CircuitBreakerMonitoringHandler : DelegatingHandler
    {
        private readonly string _serviceName;
        private readonly CircuitBreakerMonitor _monitor;

        public CircuitBreakerMonitoringHandler(string serviceName, CircuitBreakerMonitor monitor)
        {
            _serviceName = serviceName;
            _monitor = monitor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                // Record success or failure based on status code
                if (response.IsSuccessStatusCode)
                {
                    _monitor.RecordSuccess(_serviceName);
                    Console.WriteLine($"✅ SUCCESS: {_serviceName} - {request.Method} {request.RequestUri?.PathAndQuery}");
                }
                else
                {
                    _monitor.RecordFailure(_serviceName, new Exception($"HTTP {response.StatusCode}"));
                    Console.WriteLine($"⚠️ FAILURE: {_serviceName} - {response.StatusCode} - {request.Method} {request.RequestUri?.PathAndQuery}");
                }

                return response;
            }
            catch (BrokenCircuitException)
            {
                // Circuit breaker rejected the call - don't count as failure, just rejection
                _monitor.RecordRejection(_serviceName);
                Console.WriteLine($"🔴 REJECTED: {_serviceName} - Circuit breaker is OPEN");
                throw;
            }
            catch (TimeoutRejectedException ex)
            {
                _monitor.RecordTimeout(_serviceName);
                _monitor.RecordFailure(_serviceName, ex);
                Console.WriteLine($"⏱️ TIMEOUT: {_serviceName} - Request timed out");
                throw;
            }
            catch (HttpRequestException ex)
            {
                // Network or connection error
                _monitor.RecordFailure(_serviceName, ex);
                Console.WriteLine($"❌ HTTP ERROR: {_serviceName} - {ex.Message}");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                // Could be timeout or cancellation
                _monitor.RecordTimeout(_serviceName);
                _monitor.RecordFailure(_serviceName, ex);
                Console.WriteLine($"⏱️ CANCELLED/TIMEOUT: {_serviceName}");
                throw;
            }
            catch (Exception ex)
            {
                _monitor.RecordFailure(_serviceName, ex);
                Console.WriteLine($"❌ ERROR: {_serviceName} - {ex.GetType().Name}: {ex.Message}");
                throw;
            }
        }
    }
}
