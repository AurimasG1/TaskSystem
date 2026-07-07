namespace TaskSystem.Application.DTO.Requests.Users;

public record UserChangePasswordRequest(string OldPassword, string NewPassword);
