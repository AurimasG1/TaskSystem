using Microsoft.EntityFrameworkCore;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;
using TaskSystem.Infrastructure.Persistence;

namespace TaskSystem.Infrastructure.Repositories
{
    public class AdminPromotionTokenRepository : IAdminPromotionTokenRepository
    {
        private readonly AppDbContext _db;

        public AdminPromotionTokenRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<AdminPromotionToken?> GetAsync(string token)
        {
            return _db.AdminPromotionTokens.FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task AddAsync(AdminPromotionToken token)
        {
            await _db.AdminPromotionTokens.AddAsync(token);
        }

        public Task SaveChangesAsync()
        {
            return _db.SaveChangesAsync();
        }

        public Task<List<AdminPromotionToken>> GetExpiredAsync()
        {
            return _db
                .AdminPromotionTokens.Where(t => t.ExpiresAt < DateTime.UtcNow || t.Used)
                .ToListAsync();
        }

        public Task DeleteRangeAsync(List<AdminPromotionToken> tokens)
        {
            _db.AdminPromotionTokens.RemoveRange(tokens);
            return Task.CompletedTask;
        }
    }
}
