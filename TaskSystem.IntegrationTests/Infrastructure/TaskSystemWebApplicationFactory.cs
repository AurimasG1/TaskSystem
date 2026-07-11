using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskSystem.Infrastructure.Persistence;
using TaskSystem.Infrastructure.Services;

namespace TaskSystem.IntegrationTests.Infrastructure;

public sealed class TaskSystemWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public TaskSystemWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public const string TestJwtIssuer = "TaskSystemAPI";

    public const string TestJwtKey =
        "integration-tests-jwt-secret-key-" + "that-is-longer-than-thirty-two-characters";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        /*
         * Development aplinkoje HTTPS redirect nevykdomas,
         * todėl HttpClient gauna tikrą API statusą.
         */
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                Dictionary<string, string?> settings = new()
                {
                    ["ConnectionStrings:DefaultConnection"] = _connectionString,

                    ["Jwt:Key"] = TestJwtKey,
                    ["Jwt:Issuer"] = TestJwtIssuer,
                };

                configuration.AddInMemoryCollection(settings);
            }
        );

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<AppDbContext>();

            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseMySql(_connectionString, new MySqlServerVersion(new Version(8, 0, 0)));
            });

            services.RemoveAll<IConfigureOptions<HealthCheckServiceOptions>>();

            services.AddHealthChecks().AddMySql(_connectionString);

            ServiceDescriptor? cleanupService = services.SingleOrDefault(descriptor =>
                descriptor.ServiceType == typeof(IHostedService)
                && descriptor.ImplementationType == typeof(TokenCleanupService)
            );

            if (cleanupService is not null)
            {
                services.Remove(cleanupService);
            }

            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = TestJwtIssuer,

                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(TestJwtKey)
                        ),

                        ClockSkew = TimeSpan.Zero,
                    };
                }
            );
        });
    }
}
