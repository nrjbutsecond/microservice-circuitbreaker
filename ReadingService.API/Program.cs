
using Reading.Infrastructure;
using ReadingService.Api.Middleware;
using ReadingService.Core.Application.Services;
namespace ReadingService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddScoped<IReadingHistoryService, ReadingHistoryService>();
            builder.Services.AddScoped<IStatsService, StatsService>();
            builder.Services.AddHealthChecks()
            .AddNpgSql(builder.Configuration.GetConnectionString("ReadingDb")!);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
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

            app.MapControllers();

            app.MapHealthChecks("/health");

            // Log startup
            app.Logger.LogInformation("Reading-Service started");
            app.Logger.LogInformation("Circuit Breaker configured for User-Service and Comic-Service");


            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
