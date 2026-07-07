using Microsoft.EntityFrameworkCore;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Persistence;

namespace TaskSystem.Infrastructure.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context)
        : base(context) { }

    public async Task<RefreshToken?> GetByTokenAsync(string token) =>
        await _dbSet.FirstOrDefaultAsync(t => t.Token == token);

    public Task<List<RefreshToken>> GetExpiredAsync()
    {
        return _dbSet.Where(t => t.ExpiresAt < DateTime.UtcNow).ToListAsync();
    }

    public Task<List<RefreshToken>> GetRevokedAsync()
    {
        return _dbSet.Where(t => t.IsRevoked).ToListAsync();
    }

    public Task DeleteRangeAsync(List<RefreshToken> tokens)
    {
        _dbSet.RemoveRange(tokens);
        return Task.CompletedTask;
    }
}
