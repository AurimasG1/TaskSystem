using Microsoft.AspNetCore.Identity;
using Moq;
using TaskSystem.Application.Commands.Users.LoginUser;
using TaskSystem.Application.DTO.Auth;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Users;

public class LoginUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtService> _jwtMock;
    private readonly PasswordHasher<User> _hasher;
    private readonly LoginUserHandler _handler;

    public LoginUserHandlerTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtService>();
        _hasher = new PasswordHasher<User>();

        _handler = new LoginUserHandler(_userRepoMock.Object, _hasher, _jwtMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsAccessToken_WhenCredentialsAreValid()
    {
        // Arrange
        var user = new User();
        user.SetEmail("test@test.com");
        user.PasswordHash = _hasher.HashPassword(user, "Password123!");

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);

        _jwtMock.Setup(j => j.GenerateAccessToken(user)).Returns("FAKE_ACCESS_TOKEN");

        var command = new LoginUserCommand(Email: "test@test.com", Password: "Password123!");

        // Act
        LoginResponseDto result = await _handler.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("FAKE_ACCESS_TOKEN", result.AccessToken);
    }

    [Fact]
    public async Task Handle_Throws_WhenUserDoesNotExist()
    {
        _userRepoMock.Setup(r => r.GetByEmailAsync("missing@test.com")).ReturnsAsync((User?)null);

        var command = new LoginUserCommand(Email: "missing@test.com", Password: "Password123!");

        await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(command));
    }

    [Fact]
    public async Task Handle_Throws_WhenPasswordIsInvalid()
    {
        var user = new User();
        user.SetEmail("test@test.com");
        user.PasswordHash = _hasher.HashPassword(user, "CorrectPassword!");

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync(user);

        var command = new LoginUserCommand(Email: "test@test.com", Password: "WrongPassword!");

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _handler.Handle(command));
    }
}
