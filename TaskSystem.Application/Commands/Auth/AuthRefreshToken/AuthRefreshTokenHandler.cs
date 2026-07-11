using TaskSystem.Application.DTO.Responses.Auth;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Auth.AuthRefreshToken
{
    public class AuthRefreshTokenHandler
    {
        private readonly IRefreshTokenRepository _refreshRepo;
        private readonly IUserRepository _userRepo;
        private readonly IJwtService _jwt;

        public AuthRefreshTokenHandler(
            IRefreshTokenRepository refreshRepo,
            IUserRepository userRepo,
            IJwtService jwt
        )
        {
            _refreshRepo = refreshRepo;
            _userRepo = userRepo;
            _jwt = jwt;
        }

        public async Task<AuthRefreshTokenResponse> Handle(AuthRefreshTokenCommand request)
        {
            var token =
                await _refreshRepo.GetByTokenAsync(request.RefreshToken)
                ?? throw new UnauthorizedAccessException("Invalid refresh token");

            if (token.IsRevoked || token.ExpiresAt <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token is invalid or expired");
            }

            var user =
                await _userRepo.GetByIdForUpdateAsync(token.UserId)
                ?? throw new UserNotFoundException(token.UserId);

            // ROTACIJA — sugeneruojam naują refresh token
            token.IsRevoked = true;

            var newRefreshToken = _jwt.GenerateRefreshToken();

            await _refreshRepo.AddAsync(
                new RefreshToken
                {
                    UserId = user.Id,
                    Token = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    Issuer = token.Issuer,
                }
            );

            await _refreshRepo.SaveChangesAsync();

            var newAccessToken = _jwt.GenerateAccessToken(user);

            return new AuthRefreshTokenResponse(
                user.Id,
                user.Profile.Id,
                user.EmailValue,
                user.Role,
                newAccessToken,
                newRefreshToken
            );
        }
    }
}
