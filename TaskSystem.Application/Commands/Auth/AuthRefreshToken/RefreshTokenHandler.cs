using TaskSystem.Application.DTO.Responses.Auth;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Auth.AuthRefreshToken
{
    public class RefreshTokenHandler
    {
        private readonly IRefreshTokenRepository _refreshRepo;
        private readonly IUserRepository _userRepo;
        private readonly IJwtService _jwt;

        public RefreshTokenHandler(
            IRefreshTokenRepository refreshRepo,
            IUserRepository userRepo,
            IJwtService jwt
        )
        {
            _refreshRepo = refreshRepo;
            _userRepo = userRepo;
            _jwt = jwt;
        }

        public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request)
        {
            var token =
                await _refreshRepo.GetByTokenAsync(request.RefreshToken)
                ?? throw new Exception("Invalid refresh token");

            if (token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
                throw new Exception("Refresh token expired");

            var user =
                await _userRepo.GetByIdAsync(token.UserId) ?? throw new Exception("User not found");

            // ROTACIJA — sugeneruojam naują refresh token
            token.IsRevoked = true;

            var newRefreshToken = _jwt.GenerateRefreshToken();

            await _refreshRepo.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    Token = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                }
            );

            await _refreshRepo.SaveChangesAsync();

            var newAccessToken = _jwt.GenerateAccessToken(user.Id, user.EmailValue, user.Role);

            return new RefreshTokenResponse(newAccessToken, newRefreshToken);
        }
    }
}
