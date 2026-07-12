namespace TaskSystem.Application.Commands.Admin.AdminPromoteUserToAdmin;

public sealed record AdminPromoteUserToAdminCommand(int? UserId, string? Email);
