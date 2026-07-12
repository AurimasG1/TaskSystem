namespace TaskSystem.Application.Commands.Admin.AdminPromoteUserToADmin;

public enum AdminPromoteUserToAdminStatus
{
    Success,
    InvalidIdentifier,
    UserNotFound,
    AlreadyAdmin,
}
