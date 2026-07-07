namespace TaskSystem.Application.Commands.Users.UserUpdate;

public record UserUpdateCommand(int UserId, string FirstName, string LastName);
