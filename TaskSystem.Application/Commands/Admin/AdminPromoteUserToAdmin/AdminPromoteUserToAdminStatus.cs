namespace TaskSystem.Application.Commands.Admin.AdminPromoteUserToAdmin;

public enum AdminPromoteUserToAdminStatus
{
    Success,
    InvalidIdentifier,
    UserNotFound,
    AlreadyAdmin,
}
