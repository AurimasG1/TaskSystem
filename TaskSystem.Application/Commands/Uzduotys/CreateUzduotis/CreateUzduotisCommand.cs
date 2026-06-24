namespace TaskSystem.Application.Commands.Uzduotys.CreateUzduotis;

public record CreateUzduotisCommand(string Title, string? Description, int UserId);
