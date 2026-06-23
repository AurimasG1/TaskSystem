using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Persistence;
using TaskSystem.Infrastructure.Repositories;

namespace TaskSystem.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string? connectionString
    )
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new Exception(
                "Connection string nerastas. Nustatykite jį User Secrets arba Environment Variables."
            );

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
        );

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUzduotisRepository, UzduotisRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddHealthChecks().AddMySql(connectionString);

        return services;
    }
}
