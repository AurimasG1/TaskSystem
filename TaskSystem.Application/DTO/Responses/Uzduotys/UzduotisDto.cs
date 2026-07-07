namespace TaskSystem.Application.DTO.Responses.Uzduotys;

public record UzduotisDto(
    int Id,
    string Title,
    string? Description,
    int StatusId,
    int UserProfileId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
