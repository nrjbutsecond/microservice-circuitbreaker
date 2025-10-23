using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;
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

            builder.AddStandardResilienceHandler(config =>
            {
                // Circuit Breaker Configuration
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

                // Add Circuit Breaker Event Handlers for Monitoring
                if (monitor != null)
                {
                    config.CircuitBreaker.OnOpened = args =>
                    {
                        monitor.RecordEvent(new CircuitBreakerEvent
                        {
                            ServiceName = serviceName,
                            EventType = "StateChange",
                            State = "Open",
                            Details = $"Circuit opened - break duration: {args.BreakDuration.TotalSeconds}s"
                        });

                        var status = monitor.GetServiceStatus(serviceName);
                        monitor.UpdateStatus(
                            serviceName,
                            "Open",
                            status?.FailureCount ?? 0,
                            status?.SuccessCount ?? 0,
                            status?.LastFailureTime,
                            status?.LastSuccessTime,
                            DateTime.UtcNow.Add(args.BreakDuration),
                            args.BreakDuration
                        );

                        return default;
                    };

                    config.CircuitBreaker.OnClosed = args =>
                    {
                        monitor.RecordEvent(new CircuitBreakerEvent
                        {
                            ServiceName = serviceName,
                            EventType = "StateChange",
                            State = "Closed",
                            Details = "Circuit closed - normal operation resumed"
                        });

                        var status = monitor.GetServiceStatus(serviceName);
                        monitor.UpdateStatus(
                            serviceName,
                            "Closed",
                            status?.FailureCount ?? 0,
                            status?.SuccessCount ?? 0,
                            status?.LastFailureTime,
                            status?.LastSuccessTime,
                            null,
                            null
                        );

                        return default;
                    };

                    config.CircuitBreaker.OnHalfOpened = args =>
                    {
                        monitor.RecordEvent(new CircuitBreakerEvent
                        {
                            ServiceName = serviceName,
                            EventType = "StateChange",
                            State = "HalfOpen",
                            Details = "Circuit half-open - testing if service recovered"
                        });

                        var status = monitor.GetServiceStatus(serviceName);
                        monitor.UpdateStatus(
                            serviceName,
                            "HalfOpen",
                            status?.FailureCount ?? 0,
                            status?.SuccessCount ?? 0,
                            status?.LastFailureTime,
                            status?.LastSuccessTime,
                            null,
                            null
                        );

                        return default;
                    };
                }

                // Track retry events (failures)
                if (monitor != null)
                {
                    config.Retry.OnRetry = args =>
                    {
                        monitor.IncrementFailure(serviceName);

                        monitor.RecordEvent(new CircuitBreakerEvent
                        {
                            ServiceName = serviceName,
                            EventType = "Failure",
                            ErrorMessage = args.Outcome.Exception?.Message,
                            Details = $"Retry attempt {args.AttemptNumber} of {config.Retry.MaxRetryAttempts}"
                        });

                        return default;
                    };
                }
            });

            // Initialize status for this service
            if (monitor != null)
            {
                monitor.UpdateStatus(serviceName, "Closed", 0, 0, null, null, null, null);
            }

            return builder;
        }
    }

}
