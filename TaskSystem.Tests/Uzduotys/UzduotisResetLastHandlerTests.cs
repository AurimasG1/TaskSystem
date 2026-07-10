using Moq;
using TaskSystem.Application.Commands.Uzduotys.ResetLast;
using TaskSystem.Application.Commands.Uzduotys.UzduotisResetLast;
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
    public async Task Handle_WhenLastTaskExists_ResetsTaskAndReturnsMappedDto()
    {
        // Arrange
        const int expectedTaskId = 99;
        const int expectedUserProfileId = 10;
        const string originalTitle = "Title";
        const string expectedTitle = "(reset) Title";
        const int originalStatusId = 3;
        const int expectedStatusId = 1;

        var expectedCreatedAt = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc);

        var originalUpdatedAt = new DateTime(2026, 7, 2, 10, 0, 0, DateTimeKind.Utc);

        var task = new Uzduotis
        {
            Id = expectedTaskId,
            UserProfileId = expectedUserProfileId,
            Description = "Description",
            StatusId = originalStatusId,
            CreatedAt = expectedCreatedAt,
            UpdatedAt = originalUpdatedAt,
        };

        task.SetTitle(originalTitle);

        /*
         * SetTitle() pats pakeičia UpdatedAt, todėl po SetTitle()
         * grąžiname testui reikalingą seną reikšmę.
         */
        task.UpdatedAt = originalUpdatedAt;

        _taskRepoMock
            .Setup(repo => repo.GetLastByUserProfileIdForUpdateAsync(expectedUserProfileId))
            .ReturnsAsync(task);

        _repoMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        var command = new UzduotisResetLastCommand(expectedUserProfileId);

        var beforeExecution = DateTime.UtcNow;

        // Act
        var result = await _handler.Handle(command);

        var afterExecution = DateTime.UtcNow;

        // Assert: returned DTO
        Assert.Equal(expectedTaskId, result.Id);
        Assert.Equal(expectedUserProfileId, result.UserProfileId);
        Assert.Equal(expectedTitle, result.Title);
        Assert.Null(result.Description);
        Assert.Equal(expectedStatusId, result.StatusId);
        Assert.Equal(expectedCreatedAt, result.CreatedAt);

        Assert.InRange(result.UpdatedAt, beforeExecution, afterExecution);

        Assert.True(result.UpdatedAt > originalUpdatedAt);

        // Assert: modified entity
        Assert.Equal(expectedTitle, task.TitleValue);
        Assert.Null(task.Description);
        Assert.Equal(expectedStatusId, task.StatusId);
        Assert.Equal(expectedCreatedAt, task.CreatedAt);

        Assert.InRange(task.UpdatedAt, beforeExecution, afterExecution);

        // DTO turi atspindėti tą pačią entity būseną.
        Assert.Equal(task.TitleValue, result.Title);
        Assert.Equal(task.Description, result.Description);
        Assert.Equal(task.StatusId, result.StatusId);
        Assert.Equal(task.UpdatedAt, result.UpdatedAt);

        // Assert: dependency interactions
        _taskRepoMock.Verify(
            repo => repo.GetLastByUserProfileIdForUpdateAsync(expectedUserProfileId),
            Times.Once
        );

        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenLastTaskDoesNotExist_ThrowsUzduotisNotFoundException()
    {
        // Arrange
        const int userProfileId = 10;

        _taskRepoMock
            .Setup(repo => repo.GetLastByUserProfileIdForUpdateAsync(userProfileId))
            .ReturnsAsync((Uzduotis?)null);

        var command = new UzduotisResetLastCommand(userProfileId);

        // Act
        var exception = await Assert.ThrowsAsync<UzduotisNotFoundException>(() =>
            _handler.Handle(command)
        );

        // Assert
        Assert.Equal($"UserProfile {userProfileId} has no tasks.", exception.Message);

        _taskRepoMock.Verify(
            repo => repo.GetLastByUserProfileIdForUpdateAsync(userProfileId),
            Times.Once
        );

        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
}
