using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskSystem.Application.Interfaces;
using TaskSystem.Application.Services;

namespace TaskSystem.Application.DependencyInjection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IUzduotisService, UzduotisService>();
        services.AddScoped<JwtService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
