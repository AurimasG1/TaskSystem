using Microsoft.AspNetCore.Identity;
using Moq;
using TaskSystem.Application.Commands.Auth.AuthRegister;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Auth;

public class AuthRegisterHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUserProfileRepository> _profileRepoMock;
    private readonly Mock<IJwtService> _jwtMock;
    private readonly PasswordHasher<User> _passwordHasher;
    private readonly AuthRegisterHandler _handler;

    public AuthRegisterHandlerTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _profileRepoMock = new Mock<IUserProfileRepository>();
        _jwtMock = new Mock<IJwtService>();
        _passwordHasher = new PasswordHasher<User>();

        _handler = new AuthRegisterHandler(
            _userRepoMock.Object,
            _profileRepoMock.Object,
            _passwordHasher,
            _jwtMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenEmailIsAvailable_CreatesUserProfileAndReturnsAccessToken()
    {
        // Arrange
        const int expectedUserId = 10;
        const int expectedProfileId = 20;
        const string expectedEmail = "test@test.com";
        const string expectedPassword = "Password123!";
        const string expectedRole = "onboarding";
        const string expectedAccessToken = "TEST_ACCESS_TOKEN";

        var command = new AuthRegisterCommand(expectedEmail, expectedPassword);

        User? createdUser = null;
        UserProfile? createdProfile = null;

        _userRepoMock.Setup(repo => repo.GetByEmailAsync(expectedEmail)).ReturnsAsync((User?)null);

        _userRepoMock
            .Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .Callback<User>(user =>
            {
                user.Id = expectedUserId;
                createdUser = user;
            })
            .Returns(Task.CompletedTask);

        _userRepoMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        _profileRepoMock
            .Setup(repo => repo.AddAsync(It.IsAny<UserProfile>()))
            .Callback<UserProfile>(profile =>
            {
                profile.Id = expectedProfileId;
                createdProfile = profile;
            })
            .Returns(Task.CompletedTask);

        _profileRepoMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        _jwtMock
            .Setup(jwt => jwt.GenerateAccessToken(It.IsAny<User>()))
            .Returns(expectedAccessToken);

        // Act
        var result = await _handler.Handle(command);

        // Assert: returned response
        Assert.Equal(expectedUserId, result.UserId);
        Assert.Equal(expectedProfileId, result.ProfileId);
        Assert.Equal(expectedEmail, result.Email);
        Assert.Equal(expectedRole, result.Role);
        Assert.Equal(expectedAccessToken, result.AccessToken);

        // Assert: created User
        Assert.NotNull(createdUser);
        Assert.Equal(expectedUserId, createdUser.Id);
        Assert.Equal(expectedEmail, createdUser.EmailValue);
        Assert.Equal(expectedRole, createdUser.Role);
        Assert.NotEqual(expectedPassword, createdUser.PasswordHash);

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
            createdUser,
            createdUser.PasswordHash,
            expectedPassword
        );

        Assert.Equal(PasswordVerificationResult.Success, passwordVerificationResult);

        // Assert: created UserProfile
        Assert.NotNull(createdProfile);
        Assert.Equal(expectedProfileId, createdProfile.Id);
        Assert.Equal(expectedUserId, createdProfile.UserId);
        Assert.Equal(string.Empty, createdProfile.FirstName);
        Assert.Equal(string.Empty, createdProfile.LastName);

        // Handler turi prijungti profilį prie vartotojo prieš generuodamas JWT.
        Assert.Same(createdProfile, createdUser.Profile);

        // Assert: dependencies were called correctly
        _userRepoMock.Verify(repo => repo.GetByEmailAsync(expectedEmail), Times.Once);

        _userRepoMock.Verify(
            repo =>
                repo.AddAsync(
                    It.Is<User>(user =>
                        user.Id == expectedUserId
                        && user.EmailValue == expectedEmail
                        && user.Role == expectedRole
                    )
                ),
            Times.Once
        );

        _userRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

        _profileRepoMock.Verify(
            repo =>
                repo.AddAsync(
                    It.Is<UserProfile>(profile =>
                        profile.Id == expectedProfileId && profile.UserId == expectedUserId
                    )
                ),
            Times.Once
        );

        _profileRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);

        _jwtMock.Verify(
            jwt =>
                jwt.GenerateAccessToken(
                    It.Is<User>(user =>
                        user.Id == expectedUserId
                        && user.EmailValue == expectedEmail
                        && user.Role == expectedRole
                        && user.Profile.Id == expectedProfileId
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ThrowsExceptionAndDoesNotCreateUser()
    {
        // Arrange
        const string existingEmail = "exists@test.com";

        var command = new AuthRegisterCommand(existingEmail, "Password123!");

        var existingUser = new User();
        existingUser.SetEmail(existingEmail);

        _userRepoMock.Setup(repo => repo.GetByEmailAsync(existingEmail)).ReturnsAsync(existingUser);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command));

        // Assert
        Assert.Equal("User already exists", exception.Message);

        _userRepoMock.Verify(repo => repo.GetByEmailAsync(existingEmail), Times.Once);

        _userRepoMock.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);

        _userRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);

        _profileRepoMock.Verify(repo => repo.AddAsync(It.IsAny<UserProfile>()), Times.Never);

        _profileRepoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);

        _jwtMock.Verify(jwt => jwt.GenerateAccessToken(It.IsAny<User>()), Times.Never);
    }
}
