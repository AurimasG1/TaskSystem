using Moq;
using TaskSystem.Application.Commands.Uzduotys.ResetLast;
using TaskSystem.Application.Commands.Uzduotys.UzduotisResetLast;
using TaskSystem.Application.DTO.Responses.Uzduotys;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Uzduotys;

public class UzduotisResetLastHandlerTests
{
    private readonly Mock<IRepository<Uzduotis>> _repoMock;
    private readonly Mock<IUzduotisRepository> _taskRepoMock;
    private readonly UzduotisResetLastHandler _handler;

    public UzduotisResetLastHandlerTests()
    {
        _repoMock = new Mock<IRepository<Uzduotis>>();
        _taskRepoMock = new Mock<IUzduotisRepository>();
        _handler = new UzduotisResetLastHandler(_repoMock.Object, _taskRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ResetsLastTask()
    {
        var task = new Uzduotis
        {
            Id = 99,
            UserProfileId = 10,
            Description = "Desc",
            StatusId = 3,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
        };
        task.SetTitle("Title");

        _taskRepoMock.Setup(r => r.GetLastByUserProfileIdAsync(10)).ReturnsAsync(task);

        _repoMock.Setup(r => r.SaveChangesAsync());

        var command = new UzduotisResetLastCommand(10);

        UzduotisDto result = await _handler.Handle(command);

        Assert.Equal(1, result.StatusId); // Reset() sets default status
    }

    [Fact]
    public async Task Handle_Throws_WhenNoTasks()
    {
        _taskRepoMock.Setup(r => r.GetLastByUserProfileIdAsync(10)).ReturnsAsync((Uzduotis?)null);

        var command = new UzduotisResetLastCommand(10);

        await Assert.ThrowsAsync<UzduotisNotFoundException>(() => _handler.Handle(command));
    }
}
