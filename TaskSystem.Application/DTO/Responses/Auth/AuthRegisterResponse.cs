namespace TaskSystem.Application.DTO.Responses.Auth;

public record AuthRegisterResponse(int UserId, string Email, string Token);
