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

            return builder; // ✅ Trả về builder gốc
        }

        public static IHttpClientBuilder AddCustomResilienceWithLogging(
            this IHttpClientBuilder builder,
            string serviceName,
            CircuitBreakerOptions? options = null)
        {
            builder.AddCustomResilience(options);

            // Add monitoring hooks - monitor will be resolved from DI
            builder.AddHttpMessageHandler(sp =>
            {
                var monitor = sp.GetRequiredService<CircuitBreakerMonitor>();
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

                if (response.IsSuccessStatusCode)
                {
                    _monitor.RecordSuccess(_serviceName);
                }
                else
                {
                    _monitor.RecordFailure(_serviceName);
                }

                return response;
            }
            catch (BrokenCircuitException ex)
            {
                _monitor.UpdateState(_serviceName, CircuitBreakerState.Open);
                _monitor.RecordRejection(_serviceName);
                throw;
            }
            catch (TimeoutRejectedException ex)
            {
                _monitor.RecordTimeout(_serviceName);
                _monitor.RecordFailure(_serviceName, ex);
                throw;
            }
            catch (Exception ex)
            {
                _monitor.RecordFailure(_serviceName, ex);
                throw;
            }
        }
    }
}
