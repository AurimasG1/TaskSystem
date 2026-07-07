using Moq;
using TaskSystem.Application.Commands.Uzduotys.UzduotisUpdate;
using TaskSystem.Application.Common;
using TaskSystem.Application.DTO.Responses.Uzduotys;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Uzduotys;

public class UzduotisUpdateHandlerTests
{
    private readonly Mock<IRepository<Uzduotis>> _repoMock;
    private readonly Mock<IUzduotisRepository> _taskRepoMock;
    private readonly UzduotisUpdateHandler _handler;

    public UzduotisUpdateHandlerTests()
    {
        _repoMock = new Mock<IRepository<Uzduotis>>();
        _taskRepoMock = new Mock<IUzduotisRepository>();
        _handler = new UzduotisUpdateHandler(_repoMock.Object, _taskRepoMock.Object);
    }

    [Fact]
    public async Task Handle_UpdatesFields_WhenValid()
    {
        var task = new Uzduotis
        {
            Id = 99,
            UserProfileId = 10,
            Description = "Old",
            StatusId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
        };
        task.SetTitle("OldTitle");

        _taskRepoMock.Setup(r => r.GetByIdForUpdateAsync(99)).ReturnsAsync(task);

        var command = new UzduotisUpdateCommand(
            Id: 99,
            Title: Optional<string>.Some("NewTitle"),
            Description: Optional<string>.Some("NewDesc"),
            StatusId: Optional<int>.Some(2),
            UserProfileId: 10
        );

        _repoMock.Setup(r => r.SaveChangesAsync());

        UzduotisDto result = await _handler.Handle(command);

        Assert.Equal("NewTitle", result.Title);
        Assert.Equal("NewDesc", result.Description);
        Assert.Equal(2, result.StatusId);
    }

    [Fact]
    public async Task Handle_Throws_WhenTaskNotFound()
    {
        var command = new UzduotisUpdateCommand(
            99,
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<int>.None(),
            10
        );

        _taskRepoMock.Setup(r => r.GetByIdForUpdateAsync(99)).ReturnsAsync((Uzduotis?)null);

        await Assert.ThrowsAsync<UzduotisNotFoundException>(() => _handler.Handle(command));
    }

    [Fact]
    public async Task Handle_Throws_WhenUserNotOwner()
    {
        var task = new Uzduotis { Id = 99, UserProfileId = 20 };
        _taskRepoMock.Setup(r => r.GetByIdForUpdateAsync(99)).ReturnsAsync(task);

        var command = new UzduotisUpdateCommand(
            99,
            Optional<string>.None(),
            Optional<string>.None(),
            Optional<int>.None(),
            UserProfileId: 10
        );

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _handler.Handle(command));
    }
}
