using Microsoft.AspNetCore.Identity;
using TaskSystem.Application.DTO.Responses.Auth;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Auth.AuthLogin;

public class AuthLoginHandler
{
    private readonly IUserRepository _userRepo;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IJwtService _jwt;
    private readonly IRefreshTokenRepository _refreshRepo;

    public AuthLoginHandler(
        IUserRepository userRepo,
        PasswordHasher<User> passwordHasher,
        IJwtService jwt,
        IRefreshTokenRepository refreshRepo
    )
    {
        _userRepo = userRepo;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _refreshRepo = refreshRepo;
    }

    public async Task<AuthLoginResponse> Handle(AuthLoginCommand request)
    {
        // 1. Load user with Profile (IMPORTANT)
        var user =
            await _userRepo.GetByEmailForUpdateAsync(request.Email)
            ?? throw new Exception("Invalid credentials");

        // 2. Verify password
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (result != PasswordVerificationResult.Success)
            throw new Exception("Invalid credentials");

        // 3. Generate access token (NOW USING FULL USER OBJECT)
        var accessToken = _jwt.GenerateAccessToken(user);

        // 4. Generate refresh token
        var refreshToken = _jwt.GenerateRefreshToken();

        await _refreshRepo.AddAsync(
            new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Issuer = "TaskSystem",
            }
        );

        await _refreshRepo.SaveChangesAsync();

        // 5. Return response
        return new AuthLoginResponse(
            user.Id,
            user.Profile.Id,
            user.EmailValue,
            user.Role,
            accessToken,
            refreshToken
        );
    }
}
