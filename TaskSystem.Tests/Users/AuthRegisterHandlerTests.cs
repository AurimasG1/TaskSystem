using Microsoft.AspNetCore.Identity;
using Moq;
using TaskSystem.Application.Commands.Auth.AuthRegister;
using TaskSystem.Application.DTO.Responses.Auth;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Auth;

public class AuthRegisterHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IJwtService> _jwtMock;
    private readonly PasswordHasher<User> _hasher;
    private readonly AuthRegisterHandler _handler;

    public AuthRegisterHandlerTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _jwtMock = new Mock<IJwtService>();
        _hasher = new PasswordHasher<User>();

        _handler = new AuthRegisterHandler(_userRepoMock.Object, _hasher, _jwtMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesUser_WithOnboardingRole()
    {
        var command = new AuthRegisterCommand("test@test.com", "Password123!");

        _userRepoMock.Setup(r => r.GetByEmailAsync("test@test.com")).ReturnsAsync((User?)null);

        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>())).Callback<User>(u => u.Id = 10);

        _userRepoMock.Setup(r => r.SaveChangesAsync());

        _jwtMock
            .Setup(j => j.GenerateAccessToken(10, "test@test.com", "onboarding"))
            .Returns("TOKEN");

        AuthRegisterResponse result = await _handler.Handle(command);

        Assert.Equal(10, result.UserId);
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("TOKEN", result.Token);
    }

    [Fact]
    public async Task Handle_Throws_WhenUserExists()
    {
        var command = new AuthRegisterCommand("exists@test.com", "Password123!");

        _userRepoMock.Setup(r => r.GetByEmailAsync("exists@test.com")).ReturnsAsync(new User());

        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command));
    }
}
