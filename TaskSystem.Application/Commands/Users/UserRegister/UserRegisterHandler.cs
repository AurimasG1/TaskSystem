using Mapster;
using TaskSystem.Application.DTO.Responses.Users;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Users.UserRegister;

public class UserRegisterHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IUserProfileRepository _profileRepo;

    public UserRegisterHandler(IUserRepository userRepo, IUserProfileRepository profileRepo)
    {
        _userRepo = userRepo;
        _profileRepo = profileRepo;
    }

    public async Task<UserRegisterResponse> Handle(UserRegisterCommand request)
    {
        var user =
            await _userRepo.GetByIdForUpdateAsync(request.UserId)
            ?? throw new Exception("User not found");

        if (user.Role != "onboarding")
            throw new Exception("Profile already completed");

        // Create profile
        var profile = request.Adapt<UserProfile>();
        profile.UserId = user.Id;

        await _profileRepo.AddAsync(profile);

        // Change role from onboarding → user
        user.Role = "user";
        await _userRepo.UpdateAsync(user);

        // Save both
        await _profileRepo.SaveChangesAsync();
        await _userRepo.SaveChangesAsync();

        return new UserRegisterResponse(
            user.Id,
            profile.Id,
            profile.FirstName,
            profile.LastName,
            user.EmailValue,
            user.Role
        );
    }
}
