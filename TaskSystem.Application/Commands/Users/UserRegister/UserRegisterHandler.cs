using TaskSystem.Application.DTO.Responses.Auth;
using TaskSystem.Domain.Authorization;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Application.Commands.Users.UserRegister;

public sealed class UserRegisterHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly TimeProvider _timeProvider;

    public UserRegisterHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        TimeProvider timeProvider
    )
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _timeProvider = timeProvider;
    }

    public async Task<AuthLoginResponse> Handle(UserRegisterCommand request)
    {
        var user =
            await _userRepository.GetByIdForUpdateAsync(request.UserId)
            ?? throw new Exception("User not found");

        if (!string.Equals(user.Role, SystemRoles.Onboarding, StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Profile already completed");
        }

        var profile =
            user.Profile ?? throw new InvalidOperationException("User profile was not found.");

        profile.FirstName = request.FirstName;
        profile.LastName = request.LastName;

        user.Role = SystemRoles.User;

        var accessToken = _jwtService.GenerateAccessToken(user);

        var refreshTokenValue = _jwtService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = _timeProvider.GetUtcNow().UtcDateTime.AddDays(7),
            Issuer = _jwtService.Issuer,
        };

        await _refreshTokenRepository.AddAsync(refreshToken);

        // Profile, user role and refresh token are saved atomically.
        await _unitOfWork.SaveChangesAsync();

        return new AuthLoginResponse(
            user.Id,
            profile.Id,
            user.EmailValue,
            user.Role,
            accessToken,
            refreshTokenValue
        );
    }
}
