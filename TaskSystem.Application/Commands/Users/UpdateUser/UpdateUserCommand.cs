namespace TaskSystem.Application.Commands.Users.UpdateUser;

public record UpdateUserCommand(int Id, string Email, string UserName);
