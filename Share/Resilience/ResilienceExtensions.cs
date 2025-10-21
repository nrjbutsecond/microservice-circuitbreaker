using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
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
          //  builder.AddResilienceEnricher(serviceName); // Optional logging enhancer
            return builder;
        }
    }

}
