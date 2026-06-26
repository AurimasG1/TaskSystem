namespace TaskSystem.Application.DTO.Users;

public record AdminUpdateUserRequest(string Email, string UserName, string Role);
