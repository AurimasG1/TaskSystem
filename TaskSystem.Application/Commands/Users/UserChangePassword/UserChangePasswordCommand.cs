namespace TaskSystem.Application.Commands.Users.UserChangePassword;

public record UserChangePasswordCommand(int UserId, string OldPassword, string NewPassword);
