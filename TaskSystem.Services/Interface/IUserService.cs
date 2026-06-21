using TaskSystem.Entities;

namespace TaskSystem.Services.Interface
{
    public interface IUserService
    {
        Task<User?> LoginAsync(string email, string password);
        Task<User> RegisterAsync(string email, string password, string role = "user");
    }
}
