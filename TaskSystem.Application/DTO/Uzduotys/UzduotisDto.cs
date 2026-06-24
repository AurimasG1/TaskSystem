namespace TaskSystem.Application.DTO.Uzduotys;

public record UzduotisDto(
    int Id,
    string Title,
    string? Description,
    int StatusId,
    int UserId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
