using Microsoft.AspNetCore.Identity;
using Moq;
using TaskSystem.Application.Commands.Auth.AuthLogin;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Auth;

public class AuthLoginHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtService> _jwtMock;
    private readonly Mock<IRefreshTokenRepository> _refreshRepoMock;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly AuthLoginHandler _handler;

    public AuthLoginHandlerTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtService>();
        _refreshRepoMock = new Mock<IRefreshTokenRepository>();
        _passwordHasher = new PasswordHasher<User>();

        _handler = new AuthLoginHandler(
            _userRepoMock.Object,
            _passwordHasher,
            _jwtMock.Object,
            _refreshRepoMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenCredentialsAreValid_ReturnsTokensAndStoresRefreshToken()
    {
        // Arrange
        const int expectedUserId = 1;
        const int expectedProfileId = 10;
        const string expectedEmail = "test@test.com";
        const string expectedPassword = "Password123!";
        const string expectedRole = "user";
        const string expectedAccessToken = "TEST_ACCESS_TOKEN";
        const string expectedRefreshToken = "TEST_REFRESH_TOKEN";

        var user = CreateUser(
            id: expectedUserId,
            profileId: expectedProfileId,
            email: expectedEmail,
            password: expectedPassword,
            role: expectedRole
        );

        RefreshToken? savedRefreshToken = null;

        _userRepoMock
            .Setup(repo => repo.GetByEmailForUpdateAsync(expectedEmail))
            .ReturnsAsync(user);

        _jwtMock.Setup(jwt => jwt.GenerateAccessToken(user)).Returns(expectedAccessToken);

        _jwtMock.Setup(jwt => jwt.GenerateRefreshToken()).Returns(expectedRefreshToken);

        _refreshRepoMock
            .Setup(repo => repo.AddAsync(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token => savedRefreshToken = token)
            .Returns(Task.CompletedTask);

        _refreshRepoMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        var command = new AuthLoginCommand(expectedEmail, expectedPassword);

        var beforeLogin = DateTime.UtcNow;

        // Act
        var result = await _handler.Handle(command);

        var afterLogin = DateTime.UtcNow;

        // Assert: response
        Assert.Equal(expectedUserId, result.UserId);
        Assert.Equal(expectedProfileId, result.ProfileId);
        Assert.Equal(expectedEmail, result.Email);
        Assert.Equal(expectedRole, result.Role);
        Assert.Equal(expectedAccessToken, result.AccessToken);
        Assert.Equal(expectedRefreshToken, result.RefreshToken);

        // Assert: stored refresh token
        Assert.NotNull(savedRefreshToken);
        Assert.Equal(expectedUserId, savedRefreshToken.UserId);
        Assert.Equal(expectedRefreshToken, savedRefreshToken.Token);
        Assert.Equal("TaskSystem", savedRefreshToken.Issuer);
        Assert.False(savedRefreshToken.IsRevoked);

        Assert.InRange(savedRefreshToken.CreatedAt, beforeLogin, afterLogin);

        Assert.InRange(savedRefreshToken.ExpiresAt, beforeLogin.AddDays(7), afterLogin.AddDays(7));

        // Assert: dependency calls
        _userRepoMock.Verify(repo => repo.GetByEmailForUpdateAsync(expectedEmail), Times.Once);

        _jwtMock.Verify(
            jwt =>
                jwt.GenerateAccessToken(
                    It.Is<User>(actualUser =>
                        actualUser.Id == expectedUserId
                        && actualUser.Profile.Id == expectedProfileId
                        && actualUser.EmailValue == expectedEmail
                        && actualUser.Role == expectedRole
                    )
                ),
            Times.Once
        );

        _jwtMock.Verify(jwt => jwt.GenerateRefreshToken(), Times.Once);

        _refreshRepoMock.Verify(
            repo =>
                repo.AddAsync(
                    It.Is<RefreshToken>(token =>
                        token.UserId == expectedUserId
                        && token.Token == expectedRefreshToken
                        && token.Issuer == "TaskSystem"
                        && !token.IsRevoked
                    )
                ),
            Times.Once
        );

        _refreshRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsInvalidCredentialsException()
    {
        // Arrange
        const string email = "missing@test.com";

        _userRepoMock.Setup(repo => repo.GetByEmailForUpdateAsync(email)).ReturnsAsync((User?)null);

        var command = new AuthLoginCommand(email, "Password123!");

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command));

        // Assert
        Assert.Equal("Invalid credentials", exception.Message);

        _userRepoMock.Verify(repo => repo.GetByEmailForUpdateAsync(email), Times.Once);

        VerifyTokensWereNotCreated();
    }

    [Fact]
    public async Task Handle_WhenPasswordIsInvalid_ThrowsInvalidCredentialsException()
    {
        // Arrange
        const string email = "test@test.com";
        const string correctPassword = "CorrectPassword123!";
        const string incorrectPassword = "WrongPassword123!";

        var user = CreateUser(
            id: 1,
            profileId: 10,
            email: email,
            password: correctPassword,
            role: "user"
        );

        _userRepoMock.Setup(repo => repo.GetByEmailForUpdateAsync(email)).ReturnsAsync(user);

        var command = new AuthLoginCommand(email, incorrectPassword);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command));

        // Assert
        Assert.Equal("Invalid credentials", exception.Message);

        _userRepoMock.Verify(repo => repo.GetByEmailForUpdateAsync(email), Times.Once);

        VerifyTokensWereNotCreated();
    }

    private User CreateUser(int id, int profileId, string email, string password, string role)
    {
        var user = new User { Id = id, Role = role };

        user.SetEmail(email);

        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        var profile = new UserProfile
        {
            Id = profileId,
            UserId = id,
            User = user,
        };

        user.Profile = profile;

        return user;
    }

    private void VerifyTokensWereNotCreated()
    {
        _jwtMock.Verify(jwt => jwt.GenerateAccessToken(It.IsAny<User>()), Times.Never);

        _jwtMock.Verify(jwt => jwt.GenerateRefreshToken(), Times.Never);

        _refreshRepoMock.Verify(repo => repo.AddAsync(It.IsAny<RefreshToken>()), Times.Never);

        _refreshRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
}
