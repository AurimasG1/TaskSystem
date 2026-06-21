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
    Task<UzduotisResponseDto> UpdateAsync(int id, UzduotisUpdateRequestDto request, int userId);
    Task DeleteAsync(int id);
    Task<UzduotisResponseDto> ResetLastUzduotisAsync(int userId);
}
