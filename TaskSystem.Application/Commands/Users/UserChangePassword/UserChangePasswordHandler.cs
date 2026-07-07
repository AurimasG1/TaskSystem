using Microsoft.AspNetCore.Identity;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Users.UserChangePassword;

public class UserChangePasswordHandler
{
    private readonly IUserRepository _userRepo;
    private readonly PasswordHasher<User> _hasher;

    public UserChangePasswordHandler(IUserRepository userRepo, PasswordHasher<User> hasher)
    {
        _userRepo = userRepo;
        _hasher = hasher;
    }

    public async Task Handle(UserChangePasswordCommand request)
    {
        // 1. Gauti user (tracked)
        var user =
            await _userRepo.GetByIdForUpdateAsync(request.UserId)
            ?? throw new UserNotFoundException(request.UserId);

        // 2. Patikrinti seną slaptažodį
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword);

        if (result == PasswordVerificationResult.Failed)
            throw new InvalidCredentialsException();

        // 3. Hashinti naują slaptažodį
        user.PasswordHash = _hasher.HashPassword(user, request.NewPassword);

        // 4. Išsaugoti
        await _userRepo.UpdateAsync(user);
        await _userRepo.SaveChangesAsync();
    }
}
