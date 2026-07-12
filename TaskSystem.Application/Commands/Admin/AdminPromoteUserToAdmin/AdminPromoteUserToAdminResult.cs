namespace TaskSystem.Application.Commands.Admin.AdminPromoteUserToAdmin;

public sealed record AdminPromoteUserToAdminResult(
    AdminPromoteUserToAdminStatus Status,
    int? UserId = null,
    string? Email = null,
    string? PreviousRole = null
);
