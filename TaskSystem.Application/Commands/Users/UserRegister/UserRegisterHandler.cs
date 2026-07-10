using Mapster;
using TaskSystem.Application.DTO.Responses.Auth;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Users.UserRegister;

public class UserRegisterHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IUserProfileRepository _profileRepo;
    private readonly IJwtService _jwt;

    public UserRegisterHandler(
        IUserRepository userRepo,
        IUserProfileRepository profileRepo,
        IJwtService jwt
    )
    {
        _userRepo = userRepo;
        _profileRepo = profileRepo;
        _jwt = jwt;
    }

    public async Task<AuthLoginResponse> Handle(UserRegisterCommand request)
    {
        var user =
            await _userRepo.GetByIdForUpdateAsync(request.UserId)
            ?? throw new Exception("User not found");

        if (user.Role != "onboarding")
            throw new Exception("Profile already completed");

        var profile = user.Profile;
        profile.FirstName = request.FirstName;
        profile.LastName = request.LastName;
        user.Role = "user";

        await _userRepo.SaveChangesAsync();
        await _profileRepo.SaveChangesAsync();

        // Generate new JWT WITHOUT password
        var accessToken = _jwt.GenerateAccessToken(user);
        var refreshToken = _jwt.GenerateRefreshToken();

        return new AuthLoginResponse(
            user.Id,
            profile.Id,
            user.EmailValue,
            user.Role,
            accessToken,
            refreshToken
        );
    }
}
