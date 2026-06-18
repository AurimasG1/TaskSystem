using TaskSystem.Entities;

namespace TaskSystem.Repositories.Interface;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUserNameAsync(string userName);
    Task AddAsync(User user);
}
