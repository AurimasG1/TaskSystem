namespace TaskSystem.Application.Commands.Users.ChangePassword;

public record ChangePasswordCommand(int UserId, string OldPassword, string NewPassword);
