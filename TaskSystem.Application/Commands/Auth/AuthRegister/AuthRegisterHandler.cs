using Microsoft.AspNetCore.Identity;
using TaskSystem.Application.DTO.Responses.Auth;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Auth.AuthRegister;

public class AuthRegisterHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IUserProfileRepository _profileRepo;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IJwtService _jwt;

    public AuthRegisterHandler(
        IUserRepository userRepo,
        IUserProfileRepository profileRepo,
        PasswordHasher<User> passwordHasher,
        IJwtService jwt
    )
    {
        _userRepo = userRepo;
        _profileRepo = profileRepo;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
    }

    public async Task<AuthRegisterResponse> Handle(AuthRegisterCommand request)
    {
        // 1. Check if user exists
        var existing = await _userRepo.GetByEmailAsync(request.Email);
        if (existing != null)
        {
            throw new UserAlreadyExistsException(request.Email);
        }

        // 2. Create user (role = onboarding)
        var user = new User();
        user.SetEmail(request.Email);
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        user.Role = "onboarding";

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync(); // user.Id is now generated

        // 3. Create empty profile (needed for JWT profileId)
        var profile = new UserProfile
        {
            UserId = user.Id,
            FirstName = "",
            LastName = "",
        };

        await _profileRepo.AddAsync(profile);
        await _profileRepo.SaveChangesAsync(); // profile.Id is now generated

        // 4. Attach profile to user (so JWTService can read it)
        user.Profile = profile;

        // 5. Generate access token using full User object
        var accessToken = _jwt.GenerateAccessToken(user);

        // 6. Return response
        return new AuthRegisterResponse(
            user.Id,
            profile.Id,
            user.EmailValue,
            user.Role,
            accessToken
        );
    }
}
