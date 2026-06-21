using TaskSystem.Common.DTO;
using TaskSystem.Entities;
using TaskSystem.Repositories.Interface;
using TaskSystem.Services.Interface;

namespace TaskSystem.Services.Implementation;

public class UzduotisService : IUzduotisService
{
    private readonly IUzduotisRepository _repo;

    public UzduotisService(IUzduotisRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<UzduotisResponseDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return items.Select(ToDto).ToList();
    }

    public async Task<UzduotisResponseDto?> GetByIdAsync(int id)
    {
        var item = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Task not found");

        return ToDto(item);
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

    public async Task<UzduotisResponseDto?> GetLastByUserIdAsync(int userId)
    {
        var item =
            await _repo.GetLastByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("User has no tasks");

        return ToDto(item);
    }

    public async Task<List<UzduotisResponseDto>> GetTopAsync(int count)
    {
        var items = await _repo.GetTopAsync(count);
        return items.Select(ToDto).ToList();
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

        var loaded =
            await _repo.GetByIdAsync(uzduotis.Id)
            ?? throw new InvalidOperationException("Task not found after creation");

        return ToDto(loaded);
    }

    public async Task<UzduotisResponseDto> UpdateAsync(
        int id,
        UzduotisUpdateRequestDto request,
        int userId
    )
    {
        var uzduotis =
            await _repo.GetByIdForUpdateAsync(id)
            ?? throw new KeyNotFoundException("Task not found");

        if (uzduotis.UserId != userId)
            throw new UnauthorizedAccessException("You cannot edit this task.");

        uzduotis.Title = request.Title;
        uzduotis.Description = request.Description;
        uzduotis.StatusId = request.StatusId;
        uzduotis.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(uzduotis);
        await _repo.SaveChangesAsync();

        var updated =
            await _repo.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Task not found after update");
        return ToDto(updated!);
    }

    public async Task DeleteAsync(int id)
    {
        var uzduotis =
            await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Task not found");

        await _repo.DeleteAsync(uzduotis);
        await _repo.SaveChangesAsync();
    }

    public async Task<UzduotisResponseDto> ResetLastUzduotisAsync(int userId)
    {
        var uzduotis =
            await _repo.GetLastByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("User has no tasks");

        uzduotis.Title = "(reset) " + uzduotis.Title;
        uzduotis.Description = null;
        uzduotis.StatusId = 1;
        uzduotis.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(uzduotis);
        await _repo.SaveChangesAsync();

        var updated =
            await _repo.GetByIdAsync(uzduotis.Id)
            ?? throw new InvalidOperationException("Task not found after reset");

        return ToDto(updated);
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
