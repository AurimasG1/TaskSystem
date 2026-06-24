namespace TaskSystem.Application.DTO.Auth;

public record RegisterRequest(string Email, string Password, string? AdminCode);
