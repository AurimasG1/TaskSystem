namespace TaskSystem.Application.DTO.Users;

public record ChangePasswordRequest(string OldPassword, string NewPassword);
