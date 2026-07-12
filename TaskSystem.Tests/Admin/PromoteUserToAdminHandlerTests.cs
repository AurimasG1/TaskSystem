using Moq;
using TaskSystem.Application.Commands.Admin.AdminPromoteUserToAdmin;
using TaskSystem.Application.Commands.Admin.AdminPromoteUserToADmin;
using TaskSystem.Application.Commands.Admin.PromoteUserToAdmin;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Admin;

public sealed class PromoteUserToAdminHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly PromoteUserToAdminHandler _handler;

    public PromoteUserToAdminHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();

        _handler = new PromoteUserToAdminHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithEmail_PromotesUser()
    {
        // Arrange
        const int userId = 1;
        const string email = "aurimas@test.com";

        var user = CreateUser(id: userId, email: email, role: "onboarding");

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailForUpdateAsync(email))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(repository => repository.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var command = new AdminPromoteUserToAdminCommand(UserId: null, Email: email);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(AdminPromoteUserToAdminStatus.Success, result.Status);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(email, result.Email);
        Assert.Equal("onboarding", result.PreviousRole);
        Assert.Equal("admin", user.Role);

        _userRepositoryMock.Verify(
            repository => repository.GetByEmailForUpdateAsync(email),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repository => repository.GetByIdForUpdateAsync(It.IsAny<int>()),
            Times.Never
        );

        _userRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithUserId_PromotesUser()
    {
        // Arrange
        const int userId = 5;
        const string email = "user@test.com";

        var user = CreateUser(id: userId, email: email, role: "user");

        _userRepositoryMock
            .Setup(repository => repository.GetByIdForUpdateAsync(userId))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(repository => repository.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var command = new AdminPromoteUserToAdminCommand(UserId: userId, Email: null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(AdminPromoteUserToAdminStatus.Success, result.Status);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(email, result.Email);
        Assert.Equal("user", result.PreviousRole);
        Assert.Equal("admin", user.Role);

        _userRepositoryMock.Verify(
            repository => repository.GetByIdForUpdateAsync(userId),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repository => repository.GetByEmailForUpdateAsync(It.IsAny<string>()),
            Times.Never
        );

        _userRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_ReturnsUserNotFound()
    {
        // Arrange
        const string email = "missing@test.com";

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailForUpdateAsync(email))
            .ReturnsAsync((User?)null);

        var command = new AdminPromoteUserToAdminCommand(UserId: null, Email: email);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(AdminPromoteUserToAdminStatus.UserNotFound, result.Status);

        Assert.Null(result.UserId);
        Assert.Null(result.Email);
        Assert.Null(result.PreviousRole);

        _userRepositoryMock.Verify(
            repository => repository.GetByEmailForUpdateAsync(email),
            Times.Once
        );

        _userRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsAlreadyAdmin_ReturnsAlreadyAdmin()
    {
        // Arrange
        const int userId = 1;
        const string email = "admin@test.com";

        var user = CreateUser(id: userId, email: email, role: "admin");

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailForUpdateAsync(email))
            .ReturnsAsync(user);

        var command = new AdminPromoteUserToAdminCommand(UserId: null, Email: email);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(AdminPromoteUserToAdminStatus.AlreadyAdmin, result.Status);

        Assert.Equal(userId, result.UserId);
        Assert.Equal(email, result.Email);
        Assert.Equal("admin", result.PreviousRole);
        Assert.Equal("admin", user.Role);

        _userRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenBothIdentifiersProvided_ReturnsInvalidIdentifier()
    {
        // Arrange
        var command = new AdminPromoteUserToAdminCommand(UserId: 1, Email: "aurimas@test.com");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(AdminPromoteUserToAdminStatus.InvalidIdentifier, result.Status);

        VerifyRepositoryWasNotUsed();
    }

    [Fact]
    public async Task HandleAsync_WhenNoIdentifierProvided_ReturnsInvalidIdentifier()
    {
        // Arrange
        var command = new AdminPromoteUserToAdminCommand(UserId: null, Email: null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(AdminPromoteUserToAdminStatus.InvalidIdentifier, result.Status);

        VerifyRepositoryWasNotUsed();
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsWhitespace_ReturnsInvalidIdentifier()
    {
        // Arrange
        var command = new AdminPromoteUserToAdminCommand(UserId: null, Email: "   ");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(AdminPromoteUserToAdminStatus.InvalidIdentifier, result.Status);

        VerifyRepositoryWasNotUsed();
    }

    [Fact]
    public async Task HandleAsync_WithWhitespaceAroundEmail_TrimsEmailBeforeLookup()
    {
        // Arrange
        const string email = "aurimas@test.com";
        const string untrimmedEmail = "  aurimas@test.com  ";

        var user = CreateUser(id: 1, email: email, role: "user");

        _userRepositoryMock
            .Setup(repository => repository.GetByEmailForUpdateAsync(email))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(repository => repository.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var command = new AdminPromoteUserToAdminCommand(UserId: null, Email: untrimmedEmail);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Equal(AdminPromoteUserToAdminStatus.Success, result.Status);
        Assert.Equal("admin", user.Role);

        _userRepositoryMock.Verify(
            repository => repository.GetByEmailForUpdateAsync(email),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repository => repository.GetByEmailForUpdateAsync(untrimmedEmail),
            Times.Never
        );

        _userRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
    }

    private static User CreateUser(int id, string email, string role)
    {
        var user = new User { Id = id, Role = role };

        user.SetEmail(email);

        return user;
    }

    private void VerifyRepositoryWasNotUsed()
    {
        _userRepositoryMock.Verify(
            repository => repository.GetByIdForUpdateAsync(It.IsAny<int>()),
            Times.Never
        );

        _userRepositoryMock.Verify(
            repository => repository.GetByEmailForUpdateAsync(It.IsAny<string>()),
            Times.Never
        );

        _userRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
    }
}
