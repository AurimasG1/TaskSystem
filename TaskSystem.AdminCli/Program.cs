using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskSystem.AdminCli.Commands;
using TaskSystem.AdminCli.Config;
using TaskSystem.AdminCli.Services;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Persistence;
using TaskSystem.Infrastructure.Repositories;

var builder = Host.CreateApplicationBuilder(args);
Env.Load("./TaskSystem.AdminCli/.env");
var connectionString = Environment.GetEnvironmentVariable("TASKSYSTEM_DB");

// 1. Konfigūracija
var config = CliConfiguration.Load();

// 2. DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(config.ConnectionString, ServerVersion.AutoDetect(config.ConnectionString))
);

// 3. Repository
builder.Services.AddScoped<IUserRepository, UserRepository>();

// 4. Services
builder.Services.AddScoped<AdminPromotionService>();

// 5. Commands
builder.Services.AddScoped<PromoteAdminCommand>();

var app = builder.Build();

// 6. Command dispatcher
if (args.Length == 0)
{
    Console.WriteLine("Usage: admin promote --email=<email> OR --userId=<id>");
    return;
}

var command = app.Services.GetRequiredService<PromoteAdminCommand>();
await command.ExecuteAsync(args);
