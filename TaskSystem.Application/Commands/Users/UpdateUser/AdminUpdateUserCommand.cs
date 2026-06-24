namespace TaskSystem.Application.Commands.Users.UpdateUser;

public record AdminUpdateUserCommand(int Id, string Email, string UserName, string Role);
