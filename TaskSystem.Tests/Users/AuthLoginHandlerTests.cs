using Microsoft.AspNetCore.Identity;
using Moq;
using TaskSystem.Application.Commands.Auth.AuthLogin;
using TaskSystem.Application.DTO.Responses.Auth;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Auth;

public class AuthLoginHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtService> _jwtMock;
    private readonly Mock<IRefreshTokenRepository> _refreshRepoMock;
    private readonly PasswordHasher<User> _hasher;
    private readonly AuthLoginHandler _handler;

    public AuthLoginHandlerTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtService>();
        _refreshRepoMock = new Mock<IRefreshTokenRepository>();
        _hasher = new PasswordHasher<User>();

        _handler = new AuthLoginHandler(
            _userRepoMock.Object,
            _hasher,
            _jwtMock.Object,
            _refreshRepoMock.Object
        );
    }

    [Fact]
    public async Task Handle_ReturnsTokens_WhenCredentialsAreValid()
    {
        var user = new User();
        user.SetEmail("test@test.com");
        user.PasswordHash = _hasher.HashPassword(user, "Password123!");
        user.Role = "user";
        user.Id = 1;

        _userRepoMock.Setup(r => r.GetByEmailForUpdateAsync("test@test.com")).ReturnsAsync(user);

        _jwtMock
            .Setup(j => j.GenerateAccessToken(user.Id, user.EmailValue, user.Role))
            .Returns("ACCESS");
        _jwtMock.Setup(j => j.GenerateRefreshToken()).Returns("REFRESH");

        var command = new AuthLoginCommand("test@test.com", "Password123!");

        AuthLoginResponse result = await _handler.Handle(command);

        Assert.Equal("ACCESS", result.AccessToken);
        Assert.Equal("REFRESH", result.RefreshToken);
    }

    [Fact]
    public async Task Handle_Throws_WhenUserMissing()
    {
        _userRepoMock
            .Setup(r => r.GetByEmailForUpdateAsync("missing@test.com"))
            .ReturnsAsync((User?)null);

        var command = new AuthLoginCommand("missing@test.com", "Password123!");

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command));
    }

    [Fact]
    public async Task Handle_Throws_WhenPasswordInvalid()
    {
        var user = new User();
        user.SetEmail("test@test.com");
        user.PasswordHash = _hasher.HashPassword(user, "Correct!");

        _userRepoMock.Setup(r => r.GetByEmailForUpdateAsync("test@test.com")).ReturnsAsync(user);

        var command = new AuthLoginCommand("test@test.com", "Wrong!");

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command));
    }
}
