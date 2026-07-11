using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskSystem.Infrastructure.Persistence;
using Testcontainers.MySql;

namespace TaskSystem.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly MySqlContainer _mysql = new MySqlBuilder("mysql:8.0")
        .WithDatabase("tasksystem_integration_tests")
        .WithUsername("tasksystem_test")
        .WithPassword("integration-test-password")
        .Build();

    public TaskSystemWebApplicationFactory Factory { get; private set; } = null!;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        /*
         * Pirma paleidžiame DB, nes Program.cs vykdo
         * ServerVersion.AutoDetect(connectionString).
         */
        await _mysql.StartAsync();

        Factory = new TaskSystemWebApplicationFactory(_mysql.GetConnectionString());

        Client = Factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }
        );

        /*
         * Testuojame tas pačias realias EF Core migracijas,
         * kurios naudojamos aplikacijoje.
         */
        await using AsyncServiceScope scope = Factory.Services.CreateAsyncScope();

        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();

        await Factory.DisposeAsync();
        await _mysql.DisposeAsync();
    }
}
