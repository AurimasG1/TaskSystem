namespace TaskSystem.Application.DTO.Responses.Auth;

public record AuthRefreshTokenResponse(
    int UserId,
    int ProfileId,
    string Email,
    string Role,
    string AccessToken,
    string RefreshToken
);
