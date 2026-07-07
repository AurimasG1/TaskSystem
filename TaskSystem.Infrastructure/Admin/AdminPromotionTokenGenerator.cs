using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Infrastructure.Admin
{
    public class AdminPromotionTokenGenerator
    {
        private readonly IAdminPromotionTokenRepository _repo;

        public AdminPromotionTokenGenerator(IAdminPromotionTokenRepository repo)
        {
            _repo = repo;
        }

        public async Task<string> GenerateAsync()
        {
            var token = Guid.NewGuid().ToString();

            var entity = new AdminPromotionToken
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                Used = false,
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            return token;
        }
    }
}
