using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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
    private readonly IConfiguration _config;

    public AuthLoginHandler(
        IUserRepository userRepo,
        PasswordHasher<User> passwordHasher,
        IJwtService jwt,
        IRefreshTokenRepository refreshRepo,
        IConfiguration config
    )
    {
        _userRepo = userRepo;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _refreshRepo = refreshRepo;
        _config = config;
    }

    public async Task<AuthLoginResponse> Handle(AuthLoginCommand request)
    {
        var user =
            await _userRepo.GetByEmailForUpdateAsync(request.Email)
            ?? throw new Exception("Invalid credentials");

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (result != PasswordVerificationResult.Success)
            throw new Exception("Invalid credentials");

        // Access token
        var accessToken = _jwt.GenerateAccessToken(user.Id, user.EmailValue, user.Role);

        // Refresh token
        var refreshToken = _jwt.GenerateRefreshToken();

        await _refreshRepo.AddAsync(
            new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Issuer = _config["Jwt:Issuer"]!,
            }
        );

        await _refreshRepo.SaveChangesAsync();

        return new AuthLoginResponse(
            user.Id,
            user.EmailValue,
            user.Role,
            accessToken,
            refreshToken
        );
    }
}
