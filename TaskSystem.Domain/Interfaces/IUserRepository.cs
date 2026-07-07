using TaskSystem.Domain.Entities;

namespace TaskSystem.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdForUpdateAsync(int id);
    Task<User?> GetByEmailForUpdateAsync(string email);
}
