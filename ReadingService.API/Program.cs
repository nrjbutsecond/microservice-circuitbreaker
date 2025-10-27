
using Reading.Infrastructure;
using ReadingService.Api.Middleware;
using ReadingService.Core.Application.Services;
using Common.Resilience;

namespace ReadingService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            // 🔴 Register CircuitBreakerMonitor as Singleton
            builder.Services.AddSingleton<CircuitBreakerMonitor>();
            
            // 🔴 Add SignalR
            builder.Services.AddSignalR();
            
            // 🔴 Add Background Service for broadcasting stats
            builder.Services.AddHostedService<CircuitBreakerBroadcastService>();
            
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddScoped<IReadingHistoryService, ReadingHistoryService>();
            builder.Services.AddScoped<IStatsService, StatsService>();
            builder.Services.AddHealthChecks()
            .AddNpgSql(builder.Configuration.GetConnectionString("ReadingDb")!);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // Required for SignalR
                });
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseCors("AllowAll");

            app.MapControllers();

            app.MapHealthChecks("/health");

            // Log startup
            app.Logger.LogInformation("Reading-Service started");
            app.Logger.LogInformation("Circuit Breaker configured for User-Service and Comic-Service");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            
            // 🔴 Map SignalR Hub
            app.MapHub<CircuitBreakerHub>("/hubs/circuitbreaker");

            app.Run();
        }
    }
}
