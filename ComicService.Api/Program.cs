
using ComicService.Api.Middleware;
using ComicService.Core.Application.Services;
using ComicService.Infrastructure;
using Common.Resilience;

namespace ComicService.Api
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
            builder.Services.AddMemoryCache();
            
            // 🔴 Register CircuitBreakerMonitor as Singleton
            builder.Services.AddSingleton<CircuitBreakerMonitor>();
            
            // 🔴 Add SignalR
            builder.Services.AddSignalR();
            
            // 🔴 Add Background Service for broadcasting stats
            builder.Services.AddHostedService<CircuitBreakerBroadcastService>();
            
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddScoped<IComicService, ComicService.Core.Application.Services.ComicService>();
            builder.Services.AddScoped<IChapterService, ChapterService>();
            builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("ComicDb")!);
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
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
            app.UseMiddleware<ServiceAuthenticationMiddleware>(); // 🔑 API Key authentication

            app.UseCors("AllowAll");

            app.MapHealthChecks("/health");

            // Log startup
            app.Logger.LogInformation("Comic-Service started");
            app.Logger.LogInformation("Circuit Breaker configured for Reading-Service");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            
            // 🔴 Map SignalR Hub
            app.MapHub<CircuitBreakerHub>("/hubs/circuitbreaker");

            app.Run();
        }
    }
}
