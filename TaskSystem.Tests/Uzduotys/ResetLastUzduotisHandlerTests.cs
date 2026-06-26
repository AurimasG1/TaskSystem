using Moq;
using TaskSystem.Application.Commands.Uzduotys.ResetLastUzduotis;
using TaskSystem.Application.DTO.Uzduotys;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Uzduotys;

public class ResetLastUzduotisHandlerTests
{
    private readonly Mock<IRepository<Uzduotis>> _repoMock;
    private readonly Mock<IUzduotisRepository> _taskRepoMock;
    private readonly ResetLastUzduotisHandler _handler;

    public ResetLastUzduotisHandlerTests()
    {
        _repoMock = new Mock<IRepository<Uzduotis>>();
        _taskRepoMock = new Mock<IUzduotisRepository>();

        _handler = new ResetLastUzduotisHandler(_repoMock.Object, _taskRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ResetsTask_AndReturnsDto()
    {
        // Arrange
        var original = new Uzduotis
        {
            Id = 1,
            Description = "Old Desc",
            StatusId = 5,
            UserId = 5,
            UpdatedAt = DateTime.UtcNow.AddMinutes(-20),
        };
        original.SetTitle("Old");

        _taskRepoMock.Setup(r => r.GetLastByUserIdAsync(5)).ReturnsAsync(original);

        _repoMock.Setup(r => r.UpdateAsync(original));
        _repoMock.Setup(r => r.SaveChangesAsync());

        var updated = new Uzduotis
        {
            Id = 1,
            Description = null,
            StatusId = 1,
            UserId = 5,
            UpdatedAt = DateTime.UtcNow,
        };
        updated.SetTitle("(reset) Old");

        _taskRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(updated);

        var command = new ResetLastUzduotisCommand(UserId: 5);

        // Act
        UzduotisDto result = await _handler.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("(reset) Old", result.Title);
        Assert.Null(result.Description);
        Assert.Equal(1, result.StatusId);
        Assert.Equal(5, result.UserId);

        _repoMock.Verify(r => r.UpdateAsync(original), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenNoLastTaskExists()
    {
        // Arrange
        _taskRepoMock.Setup(r => r.GetLastByUserIdAsync(5)).ReturnsAsync((Uzduotis?)null);

        var command = new ResetLastUzduotisCommand(UserId: 5);

        // Act + Assert
        await Assert.ThrowsAsync<UzduotisNotFoundException>(() => _handler.Handle(command));
    }
}
