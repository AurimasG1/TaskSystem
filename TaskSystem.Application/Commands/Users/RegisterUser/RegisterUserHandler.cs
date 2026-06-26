using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using TaskSystem.Application.DTO.Auth;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Domain.ValueObjects;

namespace TaskSystem.Application.Commands.Users.RegisterUser;

public class RegisterUserHandler
{
    private readonly IUserRepository _userRepo;
    private readonly IRepository<User> _repo;
    private readonly PasswordHasher<User> _hasher;
    private readonly IConfiguration _config;

    public RegisterUserHandler(
        IUserRepository userRepo,
        IRepository<User> repo,
        PasswordHasher<User> hasher,
        IConfiguration config
    )
    {
        _userRepo = userRepo;
        _repo = repo;
        _hasher = hasher;
        _config = config;
    }

    public async Task<RegisterResponseDto> Handle(RegisterUserCommand request)
    {
        var existing = await _userRepo.GetByEmailAsync(request.Email);
        if (existing != null)
            throw new UserAlreadyExistsException(request.Email);

        string role = "user";
        string configuredAdminCode = _config["AdminSettings:AdminCode"] ?? "";

        if (
            !string.IsNullOrWhiteSpace(request.AdminCode)
            && request.AdminCode == configuredAdminCode
        )
        {
            role = "admin";
        }

        var user = new User { Role = role, CreatedAt = DateTime.UtcNow };

        user.SetEmail(request.Email.ToLowerInvariant().Trim());

        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        await _repo.AddAsync(user);
        await _repo.SaveChangesAsync();

        return new RegisterResponseDto(user.Id, user.Email.Value, user.Role);
    }
}
