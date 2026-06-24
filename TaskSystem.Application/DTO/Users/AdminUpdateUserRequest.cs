namespace TaskSystem.Application.DTO.Users;

public record AdminUpdateUserRequest(string Email, string Role, string UserName);
