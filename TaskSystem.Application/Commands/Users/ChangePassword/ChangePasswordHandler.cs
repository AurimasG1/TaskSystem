using Microsoft.AspNetCore.Identity;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Users.ChangePassword;

public class ChangePasswordHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IRepository<User> _repo;
    private readonly PasswordHasher<User> _hasher;

    public ChangePasswordHandler(
        IUserRepository userRepo,
        IRepository<User> repo,
        PasswordHasher<User> hasher
    )
    {
        _userRepo = userRepo;
        _repo = repo;
        _hasher = hasher;
    }

    public async Task Handle(ChangePasswordCommand request)
    {
        // 1. Load user with tracking
        var user =
            await _userRepo.GetByIdForUpdateAsync(request.UserId)
            ?? throw new UserNotFoundException(request.UserId);

        // 2. Verify old password
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.OldPassword);
        if (result == PasswordVerificationResult.Failed)
            throw new InvalidCredentialsException();

        // 3. Hash new password
        user.PasswordHash = _hasher.HashPassword(user, request.NewPassword);

        // 4. Save changes
        await _repo.SaveChangesAsync();
    }
}
