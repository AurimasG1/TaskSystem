using Moq;
using TaskSystem.Application.Commands.Users.UserRegister;
using TaskSystem.Application.DTO.Responses.Users;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Users;

public class UserRegisterHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUserProfileRepository> _profileRepoMock;
    private readonly UserRegisterHandler _handler;

    public UserRegisterHandlerTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _profileRepoMock = new Mock<IUserProfileRepository>();

        _handler = new UserRegisterHandler(_userRepoMock.Object, _profileRepoMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesProfile_WhenUserIsOnboarding()
    {
        var command = new UserRegisterCommand(10, "Aurimas", "Kazlauskas");

        var user = new User { Id = 10, Role = "onboarding" };
        user.SetEmail("aurimas@test.com");

        _userRepoMock.Setup(r => r.GetByIdForUpdateAsync(10)).ReturnsAsync(user);

        _profileRepoMock
            .Setup(r => r.AddAsync(It.IsAny<UserProfile>()))
            .Callback<UserProfile>(p => p.Id = 99);

        _profileRepoMock.Setup(r => r.SaveChangesAsync());
        _userRepoMock.Setup(r => r.UpdateAsync(user));
        _userRepoMock.Setup(r => r.SaveChangesAsync());

        UserDto result = await _handler.Handle(command);

        Assert.Equal(99, result.ProfileId);
        Assert.Equal(10, result.UserId);
        Assert.Equal("Aurimas", result.FirstName);
        Assert.Equal("Kazlauskas", result.LastName);
        Assert.Equal("aurimas@test.com", result.Email);
        Assert.Equal("user", result.Role);
    }
}
