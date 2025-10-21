using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserService.Core.Domain.Interfaces;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("UserDb"),
                b => b.MigrationsAssembly(typeof(UserDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
