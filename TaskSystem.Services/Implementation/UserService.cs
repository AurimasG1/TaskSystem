using System.Security.Cryptography;
using System.Text;
using TaskSystem.Entities;
using TaskSystem.Repositories.Interface;
using TaskSystem.Services.Interface;

namespace TaskSystem.Services.Implementation;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<User?> LoginAsync(string email, string password)
    {
        var user = await _repo.GetByEmailAsync(email);
        if (user == null)
            return null;

        var hash = HashPassword(password);
        if (hash != user.PasswordHash)
            return null;

        return user;
    }

    public async Task<bool> RegisterAsync(string email, string password, string role)
    {
        var existing = await _repo.GetByEmailAsync(email);
        if (existing != null)
            return false;

        var user = new User
        {
            Email = email,
            PasswordHash = HashPassword(password),
            Role = role,
        };

        await _repo.AddAsync(user);
        return true;
    }

    private string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
