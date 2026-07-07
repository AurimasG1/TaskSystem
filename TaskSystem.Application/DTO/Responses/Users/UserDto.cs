namespace TaskSystem.Application.DTO.Responses.Users;

public record UserDto(
    int UserId,
    int ProfileId,
    string FirstName,
    string LastName,
    string Email,
    string Role
);
