using Moq;
using TaskSystem.Application.Commands.Uzduotys.UzduotisCreate;
using TaskSystem.Application.DTO.Responses.Uzduotys;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Uzduotys;

public class UzduotisCreateHandlerTests
{
    private readonly Mock<IRepository<Uzduotis>> _repoMock;
    private readonly Mock<IUserProfileRepository> _profileRepoMock;
    private readonly UzduotisCreateHandler _handler;

    public UzduotisCreateHandlerTests()
    {
        _repoMock = new Mock<IRepository<Uzduotis>>();
        _profileRepoMock = new Mock<IUserProfileRepository>();
        _handler = new UzduotisCreateHandler(_repoMock.Object, _profileRepoMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesTask_WhenProfileExists()
    {
        var command = new UzduotisCreateCommand("Title", "Desc", 10);

        _profileRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new UserProfile { Id = 10 });

        _repoMock.Setup(r => r.AddAsync(It.IsAny<Uzduotis>())).Callback<Uzduotis>(t => t.Id = 99);

        _repoMock.Setup(r => r.SaveChangesAsync());

        UzduotisDto result = await _handler.Handle(command);

        Assert.Equal(99, result.Id);
        Assert.Equal("Title", result.Title);
        Assert.Equal("Desc", result.Description);
        Assert.Equal(1, result.StatusId);
        Assert.Equal(10, result.UserProfileId);
    }

    [Fact]
    public async Task Handle_Throws_WhenProfileMissing()
    {
        var command = new UzduotisCreateCommand("Title", "Desc", 10);

        _profileRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((UserProfile?)null);

        await Assert.ThrowsAsync<UserProfileNotFoundException>(() => _handler.Handle(command));
    }
}
