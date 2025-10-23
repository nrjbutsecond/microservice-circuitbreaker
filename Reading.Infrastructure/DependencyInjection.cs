using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reading.Infrastructure.ExternalServices;
using Reading.Infrastructure.Persistence;
using Reading.Infrastructure.Persistence.Repositories;
using Share.Resilience;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ReadingService.Core.Domain.Interfaces;
using ReadingService.Core.Application.Interfaces;
using Common.Resilience;
namespace Reading.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register Circuit Breaker Monitor as Singleton
            services.AddSingleton<ICircuitBreakerMonitor, CircuitBreakerMonitor>();

            // Database
            services.AddDbContext<ReadingDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("ReadingDb"),
                    b => b.MigrationsAssembly(typeof(ReadingDbContext).Assembly.FullName)));

            // Repositories
            services.AddScoped<IReadingHistoryRepository, ReadingHistoryRepository>();
            services.AddScoped<IComicStatsRepository, ComicStatsRepository>();

            // 🔴 External Services with Circuit Breaker
            AddExternalServices(services, configuration);

            return services;
        }

        private static void AddExternalServices(IServiceCollection services, IConfiguration configuration)
        {
            // Circuit Breaker Options
            var cbOptions = new CircuitBreakerOptions
            {
                SamplingDurationSeconds = 10,
                FailureRatio = 0.5,
                MinimumThroughput = 3,
                BreakDurationSeconds = 30,
                TimeoutSeconds = 3,
                RetryCount = 2,
                RetryDelaySeconds = 1
            };

            // Get the monitor instance
            var serviceProvider = services.BuildServiceProvider();
            var monitor = serviceProvider.GetRequiredService<ICircuitBreakerMonitor>();

            // 🔴 CIRCUIT BREAKER #1: User-Service
            services.AddHttpClient<IUserServiceClient, UserServiceClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["Services:User:Url"]
                    ?? "http://user-service");
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddCustomResilienceWithLogging("User-Service", monitor, cbOptions);

            // 🔴 CIRCUIT BREAKER #2: Comic-Service
            services.AddHttpClient<IComicServiceClient, ComicServiceClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["Services:Comic:Url"]
                    ?? "http://comic-service");
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddCustomResilienceWithLogging("Comic-Service", monitor, cbOptions);
        }
    }
}
