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
        await _dbSet.AsNoTracking().Include(u => u.Status).Include(u => u.User).ToListAsync();

    public async Task<Uzduotis?> GetByIdAsync(int id) =>
        await _dbSet
            .AsNoTracking()
            .Include(u => u.Status)
            .Include(u => u.User)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<Uzduotis?> GetByIdForUpdateAsync(int id) =>
        await _dbSet.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<List<Uzduotis>> GetByUserIdAsync(int userId) =>
        await _dbSet
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .Include(u => u.Status)
            .ToListAsync();

    public async Task<List<Uzduotis>> GetByUserEmailAsync(string email) =>
        await _dbSet
            .AsNoTracking()
            .Where(u => u.User.EmailValue == email)
            .Include(u => u.Status)
            .Include(u => u.User)
            .ToListAsync();

    public async Task<Uzduotis?> GetLastByUserIdAsync(int userId) =>
        await _dbSet
            .AsNoTracking()
            .Where(u => u.UserId == userId)
            .OrderByDescending(u => u.UpdatedAt)
            .Include(u => u.Status)
            .FirstOrDefaultAsync();

    public async Task<List<Uzduotis>> GetTopAsync(int count) =>
        await _dbSet
            .AsNoTracking()
            .OrderByDescending(u => u.UpdatedAt)
            .Take(count)
            .Include(u => u.Status)
            .Include(u => u.User)
            .ToListAsync();
}
