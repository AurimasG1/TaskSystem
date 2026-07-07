using Mapster;
using Microsoft.AspNetCore.Identity;
using TaskSystem.Application.DTO.Responses.Auth;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Auth.AuthRegister;

public class AuthRegisterHandler
{
    private readonly IUserRepository _userRepo;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly IJwtService _jwt;

    public AuthRegisterHandler(
        IUserRepository userRepo,
        PasswordHasher<User> passwordHasher,
        IJwtService jwt
    )
    {
        _userRepo = userRepo;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
    }

    public async Task<AuthRegisterResponse> Handle(AuthRegisterCommand request)
    {
        var existing = await _userRepo.GetByEmailAsync(request.Email);
        if (existing != null)
            throw new Exception("User already exists");

        var user = new User();
        user.SetEmail(request.Email);
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        user.Role = "onboarding";

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        var token = _jwt.GenerateAccessToken(user.Id, user.EmailValue, user.Role);

        return new AuthRegisterResponse(user.Id, user.EmailValue, token);
    }
}
