using TaskSystem.Domain.Entities;

namespace TaskSystem.Domain.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
