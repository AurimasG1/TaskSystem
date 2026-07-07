using TaskSystem.Domain.Entities;

namespace TaskSystem.Domain.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<List<RefreshToken>> GetExpiredAsync();
    Task<List<RefreshToken>> GetRevokedAsync();
    Task DeleteRangeAsync(List<RefreshToken> tokens);
}
