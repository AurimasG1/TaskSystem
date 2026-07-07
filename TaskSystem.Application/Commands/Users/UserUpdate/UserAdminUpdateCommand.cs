namespace TaskSystem.Application.Commands.Users.UserUpdate;

public record UserAdminUpdateCommand(int Id, string Email, string UserName, string Role);
