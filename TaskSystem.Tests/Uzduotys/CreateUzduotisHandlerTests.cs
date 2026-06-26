using Moq;
using TaskSystem.Application.Commands.Uzduotys.CreateUzduotis;
using TaskSystem.Application.DTO.Uzduotys;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests;

public class CreateUzduotisHandlerTests
{
    private readonly Mock<IRepository<Uzduotis>> _repoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly CreateUzduotisHandler _handler;

    public CreateUzduotisHandlerTests()
    {
        _repoMock = new Mock<IRepository<Uzduotis>>();
        _userRepoMock = new Mock<IUserRepository>();

        _handler = new CreateUzduotisHandler(_repoMock.Object, _userRepoMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesTask_AndReturnsDto()
    {
        // Arrange
        var command = new CreateUzduotisCommand(
            Title: "Test Title",
            Description: "Test Desc",
            UserId: 5
        );

        var user = new User();
        user.SetEmail("test@test.com");

        _userRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

        // Simulate EF Core assigning ID after SaveChanges
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Uzduotis>())).Callback<Uzduotis>(u => u.Id = 10);

        _repoMock.Setup(r => r.SaveChangesAsync());

        // Act
        UzduotisDto result = await _handler.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal("Test Title", result.Title);
        Assert.Equal("Test Desc", result.Description);
        Assert.Equal(1, result.StatusId);
        Assert.Equal(5, result.UserId);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Uzduotis>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
