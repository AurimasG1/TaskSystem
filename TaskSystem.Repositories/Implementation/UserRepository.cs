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

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByUserNameAsync(string userName)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.UserName == userName);
    }

    public async Task AddAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }
}
