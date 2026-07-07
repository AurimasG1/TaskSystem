using Microsoft.EntityFrameworkCore;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Persistence;

namespace TaskSystem.Infrastructure.Repositories;

public class UzduotisRepository : Repository<Uzduotis>, IUzduotisRepository
{
    public UzduotisRepository(AppDbContext context)
        : base(context) { }

    public async Task<List<Uzduotis>> GetAllAsync() =>
        await _dbSet
            .AsNoTracking()
            .Include(u => u.Status)
            .Include(u => u.UserProfile)
            .ToListAsync();

    public async Task<Uzduotis?> GetByIdAsync(int id) =>
        await _dbSet
            .AsNoTracking()
            .Include(u => u.Status)
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<Uzduotis?> GetByIdForUpdateAsync(int id) =>
        await _dbSet
            .Include(u => u.Status)
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync(u => u.Id == id);

    // ✔ Correct: Uzduotis belongs to UserProfile, not User
    public async Task<List<Uzduotis>> GetByUserProfileIdAsync(int userProfileId) =>
        await _dbSet
            .AsNoTracking()
            .Where(u => u.UserProfileId == userProfileId)
            .Include(u => u.Status)
            .Include(u => u.UserProfile)
            .ToListAsync();

    // ✔ Correct: search by email through UserProfile → User
    public async Task<List<Uzduotis>> GetByUserEmailAsync(string email) =>
        await _dbSet
            .AsNoTracking()
            .Where(u => u.UserProfile.User.EmailValue == email)
            .Include(u => u.Status)
            .Include(u => u.UserProfile)
                .ThenInclude(p => p.User)
            .ToListAsync();

    // ✔ Correct: last task by UserProfileId
    public async Task<Uzduotis?> GetLastByUserProfileIdAsync(int userProfileId) =>
        await _dbSet
            .AsNoTracking()
            .Where(u => u.UserProfileId == userProfileId)
            .OrderByDescending(u => u.UpdatedAt)
            .Include(u => u.Status)
            .Include(u => u.UserProfile)
            .FirstOrDefaultAsync();

    public async Task<List<Uzduotis>> GetTopAsync(int count) =>
        await _dbSet
            .AsNoTracking()
            .OrderByDescending(u => u.UpdatedAt)
            .Take(count)
            .Include(u => u.Status)
            .Include(u => u.UserProfile)
                .ThenInclude(p => p.User)
            .ToListAsync();
}
