using Microsoft.EntityFrameworkCore;
using TaskSystem.Common.DTO;
using TaskSystem.Data;
using TaskSystem.Entities;
using TaskSystem.Repositories.Interface;
using TaskSystem.Services.Interface;

namespace TaskSystem.Services.Implementation;

public class UzduotisService : IUzduotisService
{
    private readonly IUzduotisRepository _repo;
    private readonly AppDbContext _db;

    public UzduotisService(IUzduotisRepository repo, AppDbContext db)
    {
        _repo = repo;
        _db = db;
    }

    public async Task<Uzduotis?> GetEntityByIdAsync(int id)
    {
        return await _db.Uzduotys.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<UzduotisResponseDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return items.Select(ToDto).ToList();
    }

    public async Task<UzduotisResponseDto?> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        return item == null ? null : ToDto(item);
    }

    public async Task<UzduotisResponseDto> CreateAsync(UzduotisRequestDto request, int userId)
    {
        var uzduotis = new Uzduotis
        {
            Title = request.Title,
            Description = request.Description,
            StatusId = request.StatusId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _repo.AddAsync(uzduotis);
        await _repo.SaveChangesAsync();

        var loaded = await _db.Uzduotys.Include(u => u.Status).FirstAsync(u => u.Id == uzduotis.Id);

        return ToDto(loaded);
    }

    public async Task<bool> UpdateAsync(int id, UzduotisUpdateRequestDto request, int userId)
    {
        var uzduotis = await _repo.GetByIdAsync(id);
        if (uzduotis == null || uzduotis.UserId != userId)
            return false;

        uzduotis.Title = request.Title;
        uzduotis.Description = request.Description;
        uzduotis.StatusId = request.StatusId;
        uzduotis.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(uzduotis);
        await _repo.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var uzduotis = await _repo.GetByIdAsync(id);
        if (uzduotis == null)
            return false;

        await _repo.DeleteAsync(uzduotis);
        await _repo.SaveChangesAsync();

        return true;
    }

    public async Task<List<UzduotisResponseDto>> GetByUserIdAsync(int userId)
    {
        var items = await _repo.GetByUserIdAsync(userId);
        return items.Select(ToDto).ToList();
    }

    public async Task<List<UzduotisResponseDto>> GetByUserEmailAsync(string email)
    {
        var items = await _repo.GetByUserEmailAsync(email);
        return items.Select(ToDto).ToList();
    }

    // -----------------------------
    // LAST Uzduotis LOGIC
    // -----------------------------
    public async Task<UzduotisResponseDto?> GetLastByUserIdAsync(int userId)
    {
        var item = await _repo.GetLastByUserIdAsync(userId);
        return item == null ? null : ToDto(item);
    }

    public Task<bool> ResetLastUzduotisAsync(int userId)
    {
        return _repo.ResetLastUzduotisAsync(userId);
    }

    public async Task<List<UzduotisResponseDto>> GetTopAsync(int count)
    {
        var items = await _repo.GetTopAsync(count);
        return items.Select(ToDto).ToList();
    }

    private static UzduotisResponseDto ToDto(Uzduotis u) =>
        new UzduotisResponseDto
        {
            Id = u.Id,
            Title = u.Title,
            Description = u.Description,
            Status = u.Status.Name,
            UserId = u.UserId,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt,
        };
}
