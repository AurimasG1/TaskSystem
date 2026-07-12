using TaskSystem.Application.Commands.Admin.AdminPromoteUserToAdmin;
using TaskSystem.Application.Commands.Admin.AdminPromoteUserToADmin;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Admin.PromoteUserToAdmin;

public sealed class PromoteUserToAdminHandler
{
    private readonly IUserRepository _users;

    public PromoteUserToAdminHandler(IUserRepository users)
    {
        _users = users;
    }

    public async Task<AdminPromoteUserToAdminResult> HandleAsync(
        AdminPromoteUserToAdminCommand command,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var hasUserId = command.UserId is > 0;
        var hasEmail = !string.IsNullOrWhiteSpace(command.Email);

        // Turi būti pateiktas lygiai vienas identifikatorius.
        if (hasUserId == hasEmail)
        {
            return new AdminPromoteUserToAdminResult(
                AdminPromoteUserToAdminStatus.InvalidIdentifier
            );
        }

        var user = hasUserId
            ? await _users.GetByIdForUpdateAsync(command.UserId!.Value)
            : await _users.GetByEmailForUpdateAsync(command.Email!.Trim());

        if (user is null)
        {
            return new AdminPromoteUserToAdminResult(AdminPromoteUserToAdminStatus.UserNotFound);
        }

        if (user.IsAdmin)
        {
            return new AdminPromoteUserToAdminResult(
                AdminPromoteUserToAdminStatus.AlreadyAdmin,
                user.Id,
                user.EmailValue,
                user.Role
            );
        }

        var previousRole = user.Role;

        user.PromoteToAdmin();

        await _users.SaveChangesAsync();

        return new AdminPromoteUserToAdminResult(
            AdminPromoteUserToAdminStatus.Success,
            user.Id,
            user.EmailValue,
            previousRole
        );
    }
}
