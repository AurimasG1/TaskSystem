namespace TaskSystem.Application.DTO.Responses.Users;

public record UserRegisterResponse(
    int UserId,
    int ProfileId,
    string FirstName,
    string LastName,
    string Email,
    string Role
);
