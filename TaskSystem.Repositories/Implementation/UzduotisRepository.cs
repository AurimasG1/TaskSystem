using Microsoft.EntityFrameworkCore;
using TaskSystem.Data;
using TaskSystem.Entities;
using TaskSystem.Repositories.Interface;

namespace TaskSystem.Repositories.Implementation;

public class UzduotisRepository : IUzduotisRepository
{
    private readonly AppDbContext _db;

    public UzduotisRepository(AppDbContext db) => _db = db;

    public async Task<List<Uzduotis>> GetAllAsync() =>
        await _db.Uzduotys.AsNoTracking().Include(u => u.Status).Include(u => u.User).ToListAsync();

    public async Task<Uzduotis?> GetByIdAsync(int id) =>
        await _db
            .Uzduotys.AsNoTracking()
            .Include(u => u.Status)
            .Include(u => u.User)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<Uzduotis?> GetByIdForUpdateAsync(int id) =>
        await _db.Uzduotys.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<List<Uzduotis>> GetByUserIdAsync(int userId) =>
        await _db
            .Uzduotys.AsNoTracking()
            .Where(u => u.UserId == userId)
            .Include(u => u.Status)
            .Include(u => u.User)
            .ToListAsync();

    public async Task<List<Uzduotis>> GetByUserEmailAsync(string email) =>
        await _db
            .Uzduotys.AsNoTracking()
            .Where(u => u.User.Email == email)
            .Include(u => u.Status)
            .Include(u => u.User)
            .ToListAsync();

    public async Task<Uzduotis?> GetLastByUserIdAsync(int userId) =>
        await _db
            .Uzduotys.AsNoTracking()
            .Where(u => u.UserId == userId)
            .Include(u => u.Status)
            .Include(u => u.User)
            .OrderByDescending(u => u.UpdatedAt)
            .FirstOrDefaultAsync();

    public async Task<List<Uzduotis>> GetTopAsync(int count) =>
        await _db
            .Uzduotys.AsNoTracking()
            .Include(u => u.Status)
            .Include(u => u.User)
            .OrderByDescending(u => u.UpdatedAt)
            .Take(count)
            .ToListAsync();

    public Task AddAsync(Uzduotis uzduotis)
    {
        _db.Uzduotys.Add(uzduotis);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Uzduotis uzduotis)
    {
        _db.Uzduotys.Update(uzduotis);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Uzduotis uzduotis)
    {
        _db.Uzduotys.Attach(uzduotis);
        _db.Uzduotys.Remove(uzduotis);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
