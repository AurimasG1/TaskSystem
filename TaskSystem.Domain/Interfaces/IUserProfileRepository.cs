using TaskSystem.Domain.Entities;

namespace TaskSystem.Domain.Interfaces;

public interface IUserProfileRepository : IRepository<UserProfile>
{
    Task<UserProfile?> GetByIdAsync(int id);
    Task<UserProfile?> GetByUserIdAsync(int userId);
    Task<UserProfile?> GetByIdForUpdateAsync(int id);
    Task<UserProfile?> GetByUserIdForUpdateAsync(int userId);
}
