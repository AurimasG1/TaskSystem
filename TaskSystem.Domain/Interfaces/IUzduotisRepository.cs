using TaskSystem.Domain.Entities;

namespace TaskSystem.Domain.Interfaces;

public interface IUzduotisRepository : IRepository<Uzduotis>
{
    Task<List<Uzduotis>> GetAllAsync();
    Task<Uzduotis?> GetByIdAsync(int id);
    Task<Uzduotis?> GetByIdForUpdateAsync(int id);

    Task<List<Uzduotis>> GetByUserProfileIdAsync(int userProfileId);
    Task<List<Uzduotis>> GetByUserEmailAsync(string email);

    Task<Uzduotis?> GetLastByUserProfileIdAsync(int userProfileId);

    Task<List<Uzduotis>> GetTopAsync(int count);
}
