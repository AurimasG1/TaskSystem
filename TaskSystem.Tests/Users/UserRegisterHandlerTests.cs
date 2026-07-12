using Moq;
using TaskSystem.Application.Commands.Users.UserRegister;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Users;

public sealed class UserRegisterHandlerTests
{
    private static readonly DateTimeOffset FixedUtcNow = new(2026, 7, 12, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly UserRegisterHandler _handler;

    public UserRegisterHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _jwtServiceMock = new Mock<IJwtService>();

        var timeProvider = new FixedTimeProvider(FixedUtcNow);

        _handler = new UserRegisterHandler(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _jwtServiceMock.Object,
            timeProvider
        );
    }

    [Fact]
    public async Task Handle_WhenUserIsOnboarding_UpdatesProfileAndReturnsNewTokens()
    {
        // Arrange
        const int expectedUserId = 10;
        const int expectedProfileId = 99;

        const string expectedEmail = "aurimas@test.com";

        const string expectedFirstName = "Aurimas";

        const string expectedLastName = "Kazlauskas";

        const string expectedAccessToken = "TEST_ACCESS_TOKEN";

        const string expectedRefreshToken = "TEST_REFRESH_TOKEN";

        const string expectedIssuer = "TaskSystemAPI";

        var user = CreateOnboardingUser(
            userId: expectedUserId,
            profileId: expectedProfileId,
            email: expectedEmail
        );

        var command = new UserRegisterCommand(expectedUserId, expectedFirstName, expectedLastName);

        _userRepositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(expectedUserId))
            .ReturnsAsync(user);

        _jwtServiceMock
            .Setup(service => service.GenerateAccessToken(It.IsAny<User>()))
            .Returns(expectedAccessToken);

        _jwtServiceMock
            .Setup(service => service.GenerateRefreshToken())
            .Returns(expectedRefreshToken);

        _jwtServiceMock.SetupGet(service => service.Issuer).Returns(expectedIssuer);

        _refreshTokenRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        // Act
        var result = await _handler.Handle(command);

        // Assert: response
        Assert.Equal(expectedUserId, result.UserId);
        Assert.Equal(expectedProfileId, result.ProfileId);
        Assert.Equal(expectedEmail, result.Email);
        Assert.Equal("user", result.Role);
        Assert.Equal(expectedAccessToken, result.AccessToken);
        Assert.Equal(expectedRefreshToken, result.RefreshToken);

        // Assert: modified entities
        Assert.Equal("user", user.Role);

        Assert.NotNull(user.Profile);
        Assert.Equal(expectedFirstName, user.Profile.FirstName);
        Assert.Equal(expectedLastName, user.Profile.LastName);

        // Assert: user retrieval
        _userRepositoryMock.Verify(
            repository => repository.GetByIdForUpdateAsync(expectedUserId),
            Times.Once
        );

        // Assert: JWT generation
        _jwtServiceMock.Verify(
            service =>
                service.GenerateAccessToken(
                    It.Is<User>(actualUser =>
                        actualUser.Id == expectedUserId
                        && actualUser.Role == "user"
                        && actualUser.Profile != null
                        && actualUser.Profile.Id == expectedProfileId
                        && actualUser.Profile.FirstName == expectedFirstName
                        && actualUser.Profile.LastName == expectedLastName
                    )
                ),
            Times.Once
        );

        _jwtServiceMock.Verify(service => service.GenerateRefreshToken(), Times.Once);

        // Assert: refresh token persistence
        _refreshTokenRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.Is<RefreshToken>(token =>
                        token.UserId == expectedUserId
                        && token.Token == expectedRefreshToken
                        && token.Issuer == expectedIssuer
                        && token.ExpiresAt == FixedUtcNow.UtcDateTime.AddDays(7)
                        && !token.IsRevoked
                        && token.RevokedAt == null
                        && token.RevocationReason == null
                    )
                ),
            Times.Once
        );

        // Assert: atomic save
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsException()
    {
        // Arrange
        const int missingUserId = 404;

        var command = new UserRegisterCommand(missingUserId, "Aurimas", "Kazlauskas");

        _userRepositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(missingUserId))
            .ReturnsAsync((User?)null);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command));

        // Assert
        Assert.Equal("User not found", exception.Message);

        _userRepositoryMock.Verify(
            repository => repository.GetByIdForUpdateAsync(missingUserId),
            Times.Once
        );

        VerifyNothingWasSavedOrGenerated();
    }

    [Fact]
    public async Task Handle_WhenProfileIsAlreadyCompleted_ThrowsException()
    {
        // Arrange
        const int userId = 10;

        var user = CreateOnboardingUser(userId: userId, profileId: 99, email: "aurimas@test.com");

        user.Role = "user";

        var command = new UserRegisterCommand(userId, "Aurimas", "Kazlauskas");

        _userRepositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(userId))
            .ReturnsAsync(user);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command));

        // Assert
        Assert.Equal("Profile already completed", exception.Message);

        Assert.NotNull(user.Profile);
        Assert.Equal(string.Empty, user.Profile.FirstName);
        Assert.Equal(string.Empty, user.Profile.LastName);
        Assert.Equal("user", user.Role);

        _userRepositoryMock.Verify(
            repository => repository.GetByIdForUpdateAsync(userId),
            Times.Once
        );

        VerifyNothingWasSavedOrGenerated();
    }

    [Fact]
    public async Task Handle_WhenUserProfileDoesNotExist_ThrowsException()
    {
        // Arrange
        const int userId = 10;

        var user = new User
        {
            Id = userId,
            Role = "onboarding",
            Profile = null!,
        };

        user.SetEmail("aurimas@test.com");

        var command = new UserRegisterCommand(userId, "Aurimas", "Kazlauskas");

        _userRepositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(userId))
            .ReturnsAsync(user);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command)
        );

        // Assert
        Assert.Equal("User profile was not found.", exception.Message);

        VerifyNothingWasSavedOrGenerated();
    }

    private static User CreateOnboardingUser(int userId, int profileId, string email)
    {
        var user = new User { Id = userId, Role = "onboarding" };

        user.SetEmail(email);

        var profile = new UserProfile
        {
            Id = profileId,
            UserId = userId,
            User = user,
            FirstName = string.Empty,
            LastName = string.Empty,
        };

        user.Profile = profile;

        return user;
    }

    private void VerifyNothingWasSavedOrGenerated()
    {
        _refreshTokenRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<RefreshToken>()),
            Times.Never
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );

        _jwtServiceMock.Verify(
            service => service.GenerateAccessToken(It.IsAny<User>()),
            Times.Never
        );

        _jwtServiceMock.Verify(service => service.GenerateRefreshToken(), Times.Never);
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }
}
