using Moq;
using TaskSystem.Application.Commands.Uzduotys.UpdateUzduotis;
using TaskSystem.Application.DTO.Uzduotys;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Uzduotys;

public class UpdateUzduotisHandlerTests
{
    private readonly Mock<IRepository<Uzduotis>> _repoMock;
    private readonly Mock<IUzduotisRepository> _taskRepoMock;
    private readonly UpdateUzduotisHandler _handler;

    public UpdateUzduotisHandlerTests()
    {
        _repoMock = new Mock<IRepository<Uzduotis>>();
        _taskRepoMock = new Mock<IUzduotisRepository>();

        _handler = new UpdateUzduotisHandler(_repoMock.Object, _taskRepoMock.Object);
    }

    [Fact]
    public async Task Handle_UpdatesTask_AndReturnsDto()
    {
        // Arrange
        var original = new Uzduotis
        {
            Id = 1,
            Description = "Old Desc",
            StatusId = 1,
            UserId = 5,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-30),
        };
        original.SetTitle("Old Title");

        _taskRepoMock.Setup(r => r.GetByIdForUpdateAsync(1)).ReturnsAsync(original);

        _repoMock.Setup(r => r.SaveChangesAsync());

        var command = new UpdateUzduotisCommand(
            Id: 1,
            Title: "New Title",
            Description: "Updated Desc",
            StatusId: 2,
            UserId: 5
        );

        // Act
        UzduotisDto result = await _handler.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("New Title", result.Title);
        Assert.Equal("Updated Desc", result.Description);
        Assert.Equal(2, result.StatusId);
        Assert.Equal(5, result.UserId);

        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ThrowsUnauthorized_WhenUserDoesNotOwnTask()
    {
        // Arrange
        var task = new Uzduotis
        {
            Id = 1,
            UserId = 99, // belongs to someone else
        };
        task.SetTitle("Old");

        _taskRepoMock.Setup(r => r.GetByIdForUpdateAsync(1)).ReturnsAsync(task);

        var command = new UpdateUzduotisCommand(
            Id: 1,
            Title: "New",
            Description: "Updated",
            StatusId: 2,
            UserId: 5 // different user
        );

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command));
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        _taskRepoMock.Setup(r => r.GetByIdForUpdateAsync(1)).ReturnsAsync((Uzduotis?)null);

        var command = new UpdateUzduotisCommand(
            Id: 1,
            Title: "New",
            Description: "Updated",
            StatusId: 2,
            UserId: 5
        );

        // Act + Assert
        await Assert.ThrowsAsync<UzduotisNotFoundException>(() => _handler.Handle(command));
    }
}
