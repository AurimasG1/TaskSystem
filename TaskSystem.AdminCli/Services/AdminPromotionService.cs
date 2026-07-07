using TaskSystem.Domain.Interfaces;

namespace TaskSystem.AdminCli.Services;

public class AdminPromotionService
{
    private readonly IUserRepository _repo;

    public AdminPromotionService(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task PromoteByEmailAsync(string email)
    {
        var user = await _repo.GetByEmailForUpdateAsync(email);

        if (user == null)
        {
            Console.WriteLine($"User with email {email} not found.");
            return;
        }

        user.Role = "admin";
        await _repo.SaveChangesAsync();

        Console.WriteLine($"SUCCESS: User {email} promoted to admin.");
    }

    public async Task PromoteByIdAsync(int id)
    {
        var user = await _repo.GetByIdForUpdateAsync(id);

        if (user == null)
        {
            Console.WriteLine($"User with ID {id} not found.");
            return;
        }

        user.Role = "admin";
        await _repo.SaveChangesAsync();

        Console.WriteLine($"SUCCESS: User ID {id} promoted to admin.");
    }
}
