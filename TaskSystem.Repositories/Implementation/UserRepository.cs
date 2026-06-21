using Microsoft.EntityFrameworkCore;
using TaskSystem.Data;
using TaskSystem.Entities;
using TaskSystem.Repositories.Interface;

namespace TaskSystem.Repositories.Implementation;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(int id) =>
        await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

    public Task AddAsync(User user)
    {
        _db.Users.Add(user);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
}
