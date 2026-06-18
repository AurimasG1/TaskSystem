using Microsoft.EntityFrameworkCore;
using TaskSystem.Data;
using TaskSystem.Entities;
using TaskSystem.Repositories.Interface;

namespace TaskSystem.Repositories.Implementation;

public class UzduotisRepository : IUzduotisRepository
{
    private readonly AppDbContext _db;

    public UzduotisRepository(AppDbContext db) => _db = db;

    public async Task<List<Uzduotis>> GetAllAsync()
    {
        return await _db.Uzduotys.Include(u => u.Status).Include(u => u.User).ToListAsync();
    }

    public async Task<Uzduotis?> GetByIdAsync(int id)
    {
        return await _db
            .Uzduotys.Include(u => u.Status)
            .Include(u => u.User)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task AddAsync(Uzduotis uzduotis)
    {
        await _db.Uzduotys.AddAsync(uzduotis);
    }

    public Task UpdateAsync(Uzduotis uzduotis)
    {
        _db.Uzduotys.Update(uzduotis);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Uzduotis uzduotis)
    {
        _db.Uzduotys.Remove(uzduotis);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }

    public async Task<List<Uzduotis>> GetByUserIdAsync(int userId)
    {
        return await _db
            .Uzduotys.Where(u => u.UserId == userId)
            .Include(u => u.Status)
            .Include(u => u.User)
            .ToListAsync();
    }

    public async Task<List<Uzduotis>> GetByUserEmailAsync(string email)
    {
        return await _db
            .Uzduotys.Where(u => u.User.Email.ToLower() == email.ToLower())
            .Include(u => u.Status)
            .Include(u => u.User)
            .ToListAsync();
    }

    public async Task<Uzduotis?> GetLastByUserIdAsync(int userId)
    {
        return await _db
            .Uzduotys.Where(u => u.UserId == userId)
            .Include(u => u.Status)
            .Include(u => u.User)
            .OrderByDescending(u => u.UpdatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ResetLastUzduotisAsync(int userId)
    {
        var uzduotis = await GetLastByUserIdAsync(userId);
        if (uzduotis is null)
            return false;

        uzduotis.Title = "(reset) " + uzduotis.Title;
        uzduotis.Description = null;
        uzduotis.StatusId = 1;
        uzduotis.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<Uzduotis>> GetTopAsync(int count)
    {
        return await _db
            .Uzduotys.Include(u => u.Status)
            .Include(u => u.User)
            .OrderByDescending(u => u.UpdatedAt)
            .Take(count)
            .ToListAsync();
    }
}
