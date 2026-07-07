using Microsoft.EntityFrameworkCore;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Persistence;

namespace TaskSystem.Infrastructure.Repositories;

public class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
{
    public UserProfileRepository(AppDbContext context)
        : base(context) { }

    public async Task<UserProfile?> GetByIdAsync(int id) =>
        await _dbSet
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Uzduotys)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<UserProfile?> GetByUserIdAsync(int userId) =>
        await _dbSet
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Uzduotys)
            .FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task<UserProfile?> GetByIdForUpdateAsync(int id) =>
        await _dbSet
            .Include(p => p.User)
            .Include(p => p.Uzduotys)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<UserProfile?> GetByUserIdForUpdateAsync(int userId) =>
        await _dbSet
            .Include(p => p.User)
            .Include(p => p.Uzduotys)
            .FirstOrDefaultAsync(p => p.UserId == userId);
}
