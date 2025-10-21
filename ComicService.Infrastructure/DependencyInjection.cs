using ComicService.Core.Application.Interfaces;
using ComicService.Core.Domain.Interfaces;
using ComicService.Infrastructure.ExternalServices;
using ComicService.Infrastructure.Persistence;
using ComicService.Infrastructure.Persistence.Repositories;
using Common.Resilience;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace ComicService.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database
            services.AddDbContext<ComicDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("ComicDb"),
                    b => b.MigrationsAssembly(typeof(ComicDbContext).Assembly.FullName)));

            // Repositories
            services.AddScoped<IComicRepository, ComicRepository>();
            services.AddScoped<IChapterRepository, ChapterRepository>();

            // 🔴 External Services with Circuit Breaker
            AddExternalServices(services, configuration);

            return services;
        }

        private static void AddExternalServices(IServiceCollection services, IConfiguration configuration)
        {
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

            // 🔴 CIRCUIT BREAKER: Reading-Service
            services.AddHttpClient<IReadingServiceClient, ReadingServiceClient>(client =>
            {
                client.BaseAddress = new Uri(configuration["Services:Reading:Url"]
                    ?? "http://reading-service");
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .AddCustomResilienceWithLogging("Reading-Service", cbOptions);
        }
    }
}