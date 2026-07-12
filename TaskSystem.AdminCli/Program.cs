using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskSystem.Application.Commands.Admin.AdminPromoteUserToAdmin;
using TaskSystem.Application.Commands.Admin.AdminPromoteUserToADmin;
using TaskSystem.Application.Commands.Admin.PromoteUserToAdmin;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Persistence;
using TaskSystem.Infrastructure.Repositories;

var builder = Host.CreateApplicationBuilder(args);

// Local development secrets.
// Environment variables are added afterwards, todėl jos turi didesnį prioritetą.
builder.Configuration.AddUserSecrets<CliSecretsMarker>(optional: true).AddEnvironmentVariables();

builder.Services.AddDbContext<AppDbContext>(
    (serviceProvider, options) =>
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is not configured."
            );
        }

        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }
);

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<PromoteUserToAdminHandler>();

using var host = builder.Build();

Option<string?> emailOption = new("--email", "-e")
{
    Description = "Existing user's email address.",
};

Option<int?> userIdOption = new("--user-id", "-u") { Description = "Existing user's numeric ID." };

Command bootstrapAdminCommand = new(
    "bootstrap-admin",
    "Promotes an existing user to admin using direct database access."
)
{
    emailOption,
    userIdOption,
};

bootstrapAdminCommand.SetAction(
    (ParseResult parseResult, CancellationToken cancellationToken) =>
    {
        var email = parseResult.GetValue(emailOption);
        var userId = parseResult.GetValue(userIdOption);

        return ExecutePromotionAsync(email, userId, cancellationToken);
    }
);

RootCommand rootCommand = new("TaskSystem administrative bootstrap CLI.");

rootCommand.Subcommands.Add(bootstrapAdminCommand);

return await rootCommand.Parse(args).InvokeAsync();

async Task<int> ExecutePromotionAsync(
    string? email,
    int? userId,
    CancellationToken cancellationToken
)
{
    var hasEmail = !string.IsNullOrWhiteSpace(email);
    var hasUserId = userId is > 0;

    if (hasEmail == hasUserId)
    {
        Console.Error.WriteLine("ERROR: Specify exactly one option: --email or --user-id.");

        return ExitCodes.InvalidArguments;
    }

    try
    {
        await using var scope = host.Services.CreateAsyncScope();

        var handler = scope.ServiceProvider.GetRequiredService<PromoteUserToAdminHandler>();

        var result = await handler.HandleAsync(
            new AdminPromoteUserToAdminCommand(userId, email),
            cancellationToken
        );

        return result.Status switch
        {
            AdminPromoteUserToAdminStatus.Success => PrintSuccess(result),

            AdminPromoteUserToAdminStatus.UserNotFound => PrintError(
                "User was not found.",
                ExitCodes.UserNotFound
            ),

            AdminPromoteUserToAdminStatus.AlreadyAdmin => PrintError(
                $"User {result.Email} is already an admin.",
                ExitCodes.AlreadyAdmin
            ),

            AdminPromoteUserToAdminStatus.InvalidIdentifier => PrintError(
                "Specify exactly one valid user identifier.",
                ExitCodes.InvalidArguments
            ),

            _ => PrintError("Unexpected promotion result.", ExitCodes.SystemError),
        };
    }
    catch (OperationCanceledException)
    {
        return PrintError("Operation was cancelled.", ExitCodes.Cancelled);
    }
    catch (Exception exception)
    {
        return PrintError($"Promotion failed: {exception.Message}", ExitCodes.SystemError);
    }
}

static int PrintSuccess(AdminPromoteUserToAdminResult result)
{
    Console.WriteLine(
        $"SUCCESS: User {result.Email} "
            + $"(ID: {result.UserId}) promoted from "
            + $"'{result.PreviousRole}' to 'admin'."
    );

    return ExitCodes.Success;
}

static int PrintError(string message, int exitCode)
{
    Console.Error.WriteLine($"ERROR: {message}");
    return exitCode;
}

internal sealed class CliSecretsMarker { }

internal static class ExitCodes
{
    public const int Success = 0;
    public const int InvalidArguments = 2;
    public const int UserNotFound = 3;
    public const int AlreadyAdmin = 4;
    public const int SystemError = 10;
    public const int Cancelled = 130;
}
