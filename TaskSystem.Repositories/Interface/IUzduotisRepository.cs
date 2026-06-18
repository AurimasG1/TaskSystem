using TaskSystem.Entities;

namespace TaskSystem.Repositories.Interface;

public interface IUzduotisRepository
{
    Task<List<Uzduotis>> GetAllAsync();
    Task<Uzduotis?> GetByIdAsync(int id);
    Task AddAsync(Uzduotis uzduotis);
    Task UpdateAsync(Uzduotis uzduotis);
    Task DeleteAsync(Uzduotis uzduotis);
    Task SaveChangesAsync();

    Task<List<Uzduotis>> GetByUserIdAsync(int userId);
    Task<List<Uzduotis>> GetByUserEmailAsync(string email);
    Task<Uzduotis?> GetLastByUserIdAsync(int userId);
    Task<bool> ResetLastUzduotisAsync(int userId);
    Task<List<Uzduotis>> GetTopAsync(int count);
}
