using Moq;
using TaskSystem.Application.Commands.Users.UserRegister;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Users;

public class UserRegisterHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUserProfileRepository> _profileRepoMock;
    private readonly Mock<IJwtService> _jwtMock;
    private readonly UserRegisterHandler _handler;

    public UserRegisterHandlerTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _profileRepoMock = new Mock<IUserProfileRepository>();
        _jwtMock = new Mock<IJwtService>();

        _handler = new UserRegisterHandler(
            _userRepoMock.Object,
            _profileRepoMock.Object,
            _jwtMock.Object
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

        var user = CreateOnboardingUser(
            userId: expectedUserId,
            profileId: expectedProfileId,
            email: expectedEmail
        );

        var command = new UserRegisterCommand(expectedUserId, expectedFirstName, expectedLastName);

        _userRepoMock.Setup(repo => repo.GetByIdForUpdateAsync(expectedUserId)).ReturnsAsync(user);

        _userRepoMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        _profileRepoMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        _jwtMock
            .Setup(jwt => jwt.GenerateAccessToken(It.IsAny<User>()))
            .Returns(expectedAccessToken);

        _jwtMock.Setup(jwt => jwt.GenerateRefreshToken()).Returns(expectedRefreshToken);

        // Act
        var result = await _handler.Handle(command);

        // Assert: returned response
        Assert.Equal(expectedUserId, result.UserId);
        Assert.Equal(expectedProfileId, result.ProfileId);
        Assert.Equal(expectedEmail, result.Email);
        Assert.Equal("user", result.Role);
        Assert.Equal(expectedAccessToken, result.AccessToken);
        Assert.Equal(expectedRefreshToken, result.RefreshToken);

        // Assert: modified entities
        Assert.Equal("user", user.Role);
        Assert.Equal(expectedFirstName, user.Profile.FirstName);
        Assert.Equal(expectedLastName, user.Profile.LastName);

        // Assert: dependencies
        _userRepoMock.Verify(repo => repo.GetByIdForUpdateAsync(expectedUserId), Times.Once);

        _userRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

        _profileRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

        _jwtMock.Verify(
            jwt =>
                jwt.GenerateAccessToken(
                    It.Is<User>(actualUser =>
                        actualUser.Id == expectedUserId
                        && actualUser.Role == "user"
                        && actualUser.Profile.Id == expectedProfileId
                        && actualUser.Profile.FirstName == expectedFirstName
                        && actualUser.Profile.LastName == expectedLastName
                    )
                ),
            Times.Once
        );

        _jwtMock.Verify(jwt => jwt.GenerateRefreshToken(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsException()
    {
        // Arrange
        const int missingUserId = 404;

        var command = new UserRegisterCommand(missingUserId, "Aurimas", "Kazlauskas");

        _userRepoMock
            .Setup(repo => repo.GetByIdForUpdateAsync(missingUserId))
            .ReturnsAsync((User?)null);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command));

        // Assert
        Assert.Equal("User not found", exception.Message);

        _userRepoMock.Verify(repo => repo.GetByIdForUpdateAsync(missingUserId), Times.Once);

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

        _userRepoMock.Setup(repo => repo.GetByIdForUpdateAsync(userId)).ReturnsAsync(user);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command));

        // Assert
        Assert.Equal("Profile already completed", exception.Message);

        Assert.Equal(string.Empty, user.Profile.FirstName);
        Assert.Equal(string.Empty, user.Profile.LastName);
        Assert.Equal("user", user.Role);

        _userRepoMock.Verify(repo => repo.GetByIdForUpdateAsync(userId), Times.Once);

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
        };

        user.Profile = profile;

        return user;
    }

    private void VerifyNothingWasSavedOrGenerated()
    {
        _userRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);

        _profileRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);

        _jwtMock.Verify(jwt => jwt.GenerateAccessToken(It.IsAny<User>()), Times.Never);

        _jwtMock.Verify(jwt => jwt.GenerateRefreshToken(), Times.Never);
    }
}
