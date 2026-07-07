using TaskSystem.Domain.Entities;

namespace TaskSystem.Domain.Interfaces
{
    public interface IAdminPromotionTokenRepository
    {
        Task<AdminPromotionToken?> GetAsync(string token);
        Task AddAsync(AdminPromotionToken token);
        Task SaveChangesAsync();
        Task<List<AdminPromotionToken>> GetExpiredAsync();

        Task DeleteRangeAsync(List<AdminPromotionToken> tokens);
    }
}
