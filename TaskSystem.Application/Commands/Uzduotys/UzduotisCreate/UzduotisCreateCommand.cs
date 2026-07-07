namespace TaskSystem.Application.Commands.Uzduotys.UzduotisCreate;

public record UzduotisCreateCommand(string Title, string? Description, int UserProfileId);
