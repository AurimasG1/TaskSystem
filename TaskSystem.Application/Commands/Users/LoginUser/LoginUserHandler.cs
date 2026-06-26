using Microsoft.AspNetCore.Identity;
using TaskSystem.Application.DTO.Auth;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Users.LoginUser;

public class LoginUserHandler
{
    private readonly IUserRepository _userRepo;
    private readonly PasswordHasher<User> _hasher;
    private readonly IJwtService _jwt;

    public LoginUserHandler(IUserRepository userRepo, PasswordHasher<User> hasher, IJwtService jwt)
    {
        _userRepo = userRepo;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<LoginResponseDto> Handle(LoginUserCommand request)
    {
        var user =
            await _userRepo.GetByEmailAsync(request.Email)
            ?? throw new UserNotFoundException(request.Email);

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new InvalidCredentialsException();

        string accessToken = _jwt.GenerateAccessToken(user);
        string refreshToken = _jwt.GenerateRefreshToken();

        return new LoginResponseDto(
            user.Id,
            user.Email.Value,
            user.Role,
            accessToken,
            refreshToken
        );
    }
}
