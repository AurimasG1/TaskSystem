using Moq;
using TaskSystem.Application.Commands.Admin.AdminPromoteUserToAdmin;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Admin;

public sealed class PromoteUserToAdminHandlerTests
{
    private static readonly DateTimeOffset FixedUtcNow = new(2026, 7, 12, 12, 0, 0, TimeSpan.Zero);

    private readonly Mock<IUserRepository> _usersMock;
    private readonly Mock<IRefreshTokenRepository> _tokensMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly AdminPromoteUserToAdminHandler _handler;

    public PromoteUserToAdminHandlerTests()
    {
        _usersMock = new Mock<IUserRepository>();
        _tokensMock = new Mock<IRefreshTokenRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var timeProvider = new FixedTimeProvider(FixedUtcNow);

        _handler = new AdminPromoteUserToAdminHandler(
            _usersMock.Object,
            _tokensMock.Object,
            _unitOfWorkMock.Object,
            timeProvider
        );
    }

    [Fact]
    public async Task HandleAsync_WithEmail_PromotesUserAndRevokesTokens()
    {
        // Arrange
        const int userId = 1;
        const string email = "aurimas@test.com";

        var user = CreateUser(userId, email, "onboarding");

        var firstToken = CreateRefreshToken(userId, "first-token");

        var secondToken = CreateRefreshToken(userId, "second-token");

        _usersMock
            .Setup(repository => repository.GetByEmailForUpdateAsync(email))
            .ReturnsAsync(user);

        _tokensMock
            .Setup(repository =>
                repository.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([firstToken, secondToken]);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var command = new AdminPromoteUserToAdminCommand(UserId: null, Email: email);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(AdminPromoteUserToAdminStatus.Success, result.Status);

        Assert.Equal(userId, result.UserId);
        Assert.Equal(email, result.Email);
        Assert.Equal("onboarding", result.PreviousRole);
        Assert.Equal("admin", user.Role);

        AssertTokenRevoked(firstToken);
        AssertTokenRevoked(secondToken);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WithUserId_PromotesUser()
    {
        // Arrange
        const int userId = 5;
        const string email = "user@test.com";

        var user = CreateUser(userId, email, "user");

        _usersMock.Setup(repository => repository.GetByIdForUpdateAsync(userId)).ReturnsAsync(user);

        _tokensMock
            .Setup(repository =>
                repository.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new AdminPromoteUserToAdminCommand(UserId: userId, Email: null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(AdminPromoteUserToAdminStatus.Success, result.Status);

        Assert.Equal("admin", user.Role);
        Assert.Equal("user", result.PreviousRole);

        _usersMock.Verify(repository => repository.GetByIdForUpdateAsync(userId), Times.Once);

        _usersMock.Verify(
            repository => repository.GetByEmailForUpdateAsync(It.IsAny<string>()),
            Times.Never
        );

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_ReturnsUserNotFound()
    {
        // Arrange
        const string email = "missing@test.com";

        _usersMock
            .Setup(repository => repository.GetByEmailForUpdateAsync(email))
            .ReturnsAsync((User?)null);

        var command = new AdminPromoteUserToAdminCommand(UserId: null, Email: email);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(AdminPromoteUserToAdminStatus.UserNotFound, result.Status);

        _tokensMock.Verify(
            repository =>
                repository.GetActiveByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        VerifyNothingWasSaved();
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadyAdmin_ReturnsAlreadyAdmin()
    {
        // Arrange
        const int userId = 1;
        const string email = "admin@test.com";

        var user = CreateUser(userId, email, "admin");

        _usersMock
            .Setup(repository => repository.GetByEmailForUpdateAsync(email))
            .ReturnsAsync(user);

        var command = new AdminPromoteUserToAdminCommand(UserId: null, Email: email);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(AdminPromoteUserToAdminStatus.AlreadyAdmin, result.Status);

        Assert.Equal("admin", user.Role);

        _tokensMock.Verify(
            repository =>
                repository.GetActiveByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        VerifyNothingWasSaved();
    }

    [Fact]
    public async Task HandleAsync_WhenBothIdentifiersProvided_ReturnsInvalidIdentifier()
    {
        var command = new AdminPromoteUserToAdminCommand(UserId: 1, Email: "aurimas@test.com");

        var result = await _handler.HandleAsync(command);

        Assert.Equal(AdminPromoteUserToAdminStatus.InvalidIdentifier, result.Status);

        VerifyRepositoriesWereNotUsed();
    }

    [Fact]
    public async Task HandleAsync_WhenNoIdentifierProvided_ReturnsInvalidIdentifier()
    {
        var command = new AdminPromoteUserToAdminCommand(UserId: null, Email: null);

        var result = await _handler.HandleAsync(command);

        Assert.Equal(AdminPromoteUserToAdminStatus.InvalidIdentifier, result.Status);

        VerifyRepositoriesWereNotUsed();
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsWhitespace_ReturnsInvalidIdentifier()
    {
        var command = new AdminPromoteUserToAdminCommand(UserId: null, Email: "   ");

        var result = await _handler.HandleAsync(command);

        Assert.Equal(AdminPromoteUserToAdminStatus.InvalidIdentifier, result.Status);

        VerifyRepositoriesWereNotUsed();
    }

    [Fact]
    public async Task HandleAsync_TrimsEmailBeforeLookup()
    {
        // Arrange
        const int userId = 1;
        const string email = "aurimas@test.com";

        var user = CreateUser(userId, email, "user");

        _usersMock
            .Setup(repository => repository.GetByEmailForUpdateAsync(email))
            .ReturnsAsync(user);

        _tokensMock
            .Setup(repository =>
                repository.GetActiveByUserIdAsync(userId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync([]);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new AdminPromoteUserToAdminCommand(UserId: null, Email: $"  {email}  ");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(AdminPromoteUserToAdminStatus.Success, result.Status);

        _usersMock.Verify(repository => repository.GetByEmailForUpdateAsync(email), Times.Once);
    }

    private static User CreateUser(int id, string email, string role)
    {
        var user = new User { Id = id, Role = role };

        user.SetEmail(email);

        return user;
    }

    private static RefreshToken CreateRefreshToken(int userId, string token)
    {
        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            Issuer = "TaskSystem",
            ExpiresAt = FixedUtcNow.UtcDateTime.AddDays(7),
        };
    }

    private static void AssertTokenRevoked(RefreshToken token)
    {
        Assert.True(token.IsRevoked);

        Assert.Equal(FixedUtcNow.UtcDateTime, token.RevokedAt);

        Assert.Equal("User promoted to admin", token.RevocationReason);
    }

    private void VerifyNothingWasSaved()
    {
        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    private void VerifyRepositoriesWereNotUsed()
    {
        _usersMock.Verify(
            repository => repository.GetByIdForUpdateAsync(It.IsAny<int>()),
            Times.Never
        );

        _usersMock.Verify(
            repository => repository.GetByEmailForUpdateAsync(It.IsAny<string>()),
            Times.Never
        );

        _tokensMock.Verify(
            repository =>
                repository.GetActiveByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        VerifyNothingWasSaved();
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
