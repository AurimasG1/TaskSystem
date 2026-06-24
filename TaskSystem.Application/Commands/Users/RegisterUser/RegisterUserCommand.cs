namespace TaskSystem.Application.Commands.Users.RegisterUser;

public record RegisterUserCommand(string Email, string Password, string? AdminCode);
