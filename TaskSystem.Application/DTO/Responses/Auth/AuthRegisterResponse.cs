namespace TaskSystem.Application.DTO.Responses.Auth;

public record AuthRegisterResponse(
    int UserId,
    int ProfileId,
    string Email,
    string Role,
    string AccessToken
);
