using TaskSystem.Common.DTO;
using TaskSystem.Entities;

namespace TaskSystem.Services.Interface;

public interface IUzduotisService
{
    Task<List<UzduotisResponseDto>> GetAllAsync();
    Task<UzduotisResponseDto?> GetByIdAsync(int id);
    Task<List<UzduotisResponseDto>> GetByUserIdAsync(int userId);
    Task<List<UzduotisResponseDto>> GetByUserEmailAsync(string email);
    Task<UzduotisResponseDto?> GetLastByUserIdAsync(int userId);
    Task<List<UzduotisResponseDto>> GetTopAsync(int count);
    Task<UzduotisResponseDto> CreateAsync(UzduotisRequestDto request, int userId);
    Task<bool> UpdateAsync(int id, UzduotisUpdateRequestDto request, int userId);
    Task<bool> DeleteAsync(int id);
    Task<bool> ResetLastUzduotisAsync(int userId);
}
