using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using TaskSystem.Application.Commands.Users.RegisterUser;
using TaskSystem.Application.DTO.Auth;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Users;

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRepository<User>> _repoMock;
    private readonly PasswordHasher<User> _hasher;
    private readonly Mock<IConfiguration> _configMock;
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _repoMock = new Mock<IRepository<User>>();
        _hasher = new PasswordHasher<User>();
        _configMock = new Mock<IConfiguration>();

        _handler = new RegisterUserHandler(
            _userRepoMock.Object,
            _repoMock.Object,
            _hasher,
            _configMock.Object
        );
    }

    [Fact]
    public async Task Handle_RegistersNormalUser_WhenAdminCodeIsMissing()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@test.com",
            Password: "Password123!",
            AdminCode: null
        );

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync((User?)null);

        _configMock.Setup(c => c["AdminSettings:AdminCode"]).Returns("SECRET_ADMIN");

        _repoMock.Setup(r => r.AddAsync(It.IsAny<User>())).Callback<User>(u => u.Id = 10);

        _repoMock.Setup(r => r.SaveChangesAsync());

        // Act
        RegisterResponseDto result = await _handler.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("user", result.Role);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_RegistersAdmin_WhenAdminCodeMatches()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "admin@test.com",
            Password: "Password123!",
            AdminCode: "ADMIN123"
        );

        _userRepoMock.Setup(r => r.GetByEmailAsync("admin@test.com")).ReturnsAsync((User?)null);

        _configMock.Setup(c => c["AdminSettings:AdminCode"]).Returns("ADMIN123");

        _repoMock.Setup(r => r.AddAsync(It.IsAny<User>())).Callback<User>(u => u.Id = 20);

        _repoMock.Setup(r => r.SaveChangesAsync());

        // Act
        RegisterResponseDto result = await _handler.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20, result.Id);
        Assert.Equal("admin@test.com", result.Email);
        Assert.Equal("admin", result.Role);
    }

    [Fact]
    public async Task Handle_Throws_WhenUserAlreadyExists()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "exists@test.com",
            Password: "Password123!",
            AdminCode: null
        );

        _userRepoMock.Setup(r => r.GetByEmailAsync("exists@test.com")).ReturnsAsync(new User());

        // Act + Assert
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() => _handler.Handle(command));
    }
}
