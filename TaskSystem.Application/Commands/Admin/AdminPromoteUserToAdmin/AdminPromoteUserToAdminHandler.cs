using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Admin.AdminPromoteUserToAdmin;

public sealed class AdminPromoteUserToAdminHandler
{
    private const string TokenRevocationReason = "User promoted to admin";

    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public AdminPromoteUserToAdminHandler(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider
    )
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
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

        var activeRefreshTokens = await _refreshTokens.GetActiveByUserIdAsync(
            user.Id,
            cancellationToken
        );

        var revokedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;

        user.PromoteToAdmin();

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.Revoke(revokedAtUtc, TokenRevocationReason);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AdminPromoteUserToAdminResult(
            AdminPromoteUserToAdminStatus.Success,
            user.Id,
            user.EmailValue,
            previousRole
        );
    }
}
