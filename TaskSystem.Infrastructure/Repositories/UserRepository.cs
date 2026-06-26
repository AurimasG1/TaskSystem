using Microsoft.EntityFrameworkCore;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Persistence;

namespace TaskSystem.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context)
        : base(context) { }

    public async Task<User?> GetByIdAsync(int id) =>
        await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email.Value == email);

    public async Task<User?> GetByIdForUpdateAsync(int id) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailForUpdateAsync(string email) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Email.Value == email);
}
