using Microsoft.EntityFrameworkCore;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Persistence;

namespace TaskSystem.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context)
        : base(context) { }

    // ---------------------------------------------------------
    // READ-ONLY QUERY (always AsNoTracking)
    // ---------------------------------------------------------

    public async Task<User?> GetByIdAsync(int id) =>
        await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

    // ---------------------------------------------------------
    // UPDATE SCENARIUS (tracking ON)
    // ---------------------------------------------------------

    public async Task<User?> GetByIdForUpdateAsync(int id) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailForUpdateAsync(string email) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
}
