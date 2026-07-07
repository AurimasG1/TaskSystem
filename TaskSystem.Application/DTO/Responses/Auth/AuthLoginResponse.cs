namespace TaskSystem.Application.DTO.Responses.Auth;

public record AuthLoginResponse(
    int Id,
    string Email,
    string Role,
    string AccessToken,
    string RefreshToken
);
