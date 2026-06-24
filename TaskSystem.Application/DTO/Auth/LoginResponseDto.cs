namespace TaskSystem.Application.DTO.Auth;

public record LoginResponseDto(
    int Id,
    string Email,
    string Role,
    string AccessToken,
    string RefreshToken
);
