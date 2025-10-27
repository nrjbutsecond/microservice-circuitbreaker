
using UserService.Api.Middleware;
using UserService.Core.Application.Service;
using HealthChecks.NpgSql;
using Common.Resilience;

namespace UserService.Api
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
            builder.Services.AddScoped<IUserService, UserService.Core.Application.Service.UserService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddHealthChecks()
           .AddNpgSql(builder.Configuration.GetConnectionString("UserDb")!);

            // CORS
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
            
            app.MapHealthChecks("/health");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();
            
            // 🔴 Map SignalR Hub
            app.MapHub<CircuitBreakerHub>("/hubs/circuitbreaker");

            app.Run();
        }
    }
}
