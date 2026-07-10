using Moq;
using TaskSystem.Application.Commands.Uzduotys.UzduotisUpdate;
using TaskSystem.Application.Common;
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
    public async Task Handle_WhenRequestIsValid_UpdatesProvidedFieldsAndReturnsMappedDto()
    {
        // Arrange
        const int taskId = 99;
        const int ownerProfileId = 10;

        var originalCreatedAt = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc);

        var originalUpdatedAt = new DateTime(2026, 7, 2, 10, 0, 0, DateTimeKind.Utc);

        var task = CreateTask(
            id: taskId,
            userProfileId: ownerProfileId,
            title: "Old title",
            description: "Old description",
            statusId: 1,
            createdAt: originalCreatedAt,
            updatedAt: originalUpdatedAt
        );

        _taskRepoMock.Setup(repo => repo.GetByIdForUpdateAsync(taskId)).ReturnsAsync(task);

        _repoMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        var command = new UzduotisUpdateCommand(
            Id: taskId,
            Title: Optional<string>.Some("New title"),
            Description: Optional<string>.Some("New description"),
            StatusId: Optional<int>.Some(2),
            UserProfileId: ownerProfileId
        );

        var beforeExecution = DateTime.UtcNow;

        // Act
        var result = await _handler.Handle(command);

        var afterExecution = DateTime.UtcNow;

        // Assert: entity was updated
        Assert.Equal("New title", task.TitleValue);
        Assert.Equal("New description", task.Description);
        Assert.Equal(2, task.StatusId);
        Assert.Equal(ownerProfileId, task.UserProfileId);
        Assert.Equal(originalCreatedAt, task.CreatedAt);

        Assert.InRange(task.UpdatedAt, beforeExecution, afterExecution);

        Assert.True(task.UpdatedAt > originalUpdatedAt);

        // Assert: returned DTO
        Assert.Equal(taskId, result.Id);
        Assert.Equal("New title", result.Title);
        Assert.Equal("New description", result.Description);
        Assert.Equal(2, result.StatusId);
        Assert.Equal(ownerProfileId, result.UserProfileId);
        Assert.Equal(originalCreatedAt, result.CreatedAt);
        Assert.Equal(task.UpdatedAt, result.UpdatedAt);

        // Assert: dependencies
        _taskRepoMock.Verify(repo => repo.GetByIdForUpdateAsync(taskId), Times.Once);

        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOnlyDescriptionIsProvided_UpdatesDescriptionAndPreservesOtherFields()
    {
        // Arrange
        const int taskId = 99;
        const int ownerProfileId = 10;

        var originalCreatedAt = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc);

        var originalUpdatedAt = new DateTime(2026, 7, 2, 10, 0, 0, DateTimeKind.Utc);

        var task = CreateTask(
            id: taskId,
            userProfileId: ownerProfileId,
            title: "Original title",
            description: "Original description",
            statusId: 1,
            createdAt: originalCreatedAt,
            updatedAt: originalUpdatedAt
        );

        _taskRepoMock.Setup(repo => repo.GetByIdForUpdateAsync(taskId)).ReturnsAsync(task);

        _repoMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        var command = new UzduotisUpdateCommand(
            Id: taskId,
            Title: Optional<string>.None(),
            Description: Optional<string>.Some("Changed description"),
            StatusId: Optional<int>.None(),
            UserProfileId: ownerProfileId
        );

        // Act
        var result = await _handler.Handle(command);

        // Assert
        Assert.Equal("Original title", task.TitleValue);
        Assert.Equal("Changed description", task.Description);
        Assert.Equal(1, task.StatusId);
        Assert.Equal(originalCreatedAt, task.CreatedAt);
        Assert.True(task.UpdatedAt > originalUpdatedAt);

        Assert.Equal("Original title", result.Title);
        Assert.Equal("Changed description", result.Description);
        Assert.Equal(1, result.StatusId);

        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDescriptionIsExplicitlyNull_ClearsDescription()
    {
        // Arrange
        const int taskId = 99;
        const int ownerProfileId = 10;

        var task = CreateTask(
            id: taskId,
            userProfileId: ownerProfileId,
            title: "Title",
            description: "Description to remove",
            statusId: 1
        );

        _taskRepoMock.Setup(repo => repo.GetByIdForUpdateAsync(taskId)).ReturnsAsync(task);

        _repoMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        var command = new UzduotisUpdateCommand(
            Id: taskId,
            Title: Optional<string>.None(),
            Description: Optional<string>.Some(null!),
            StatusId: Optional<int>.None(),
            UserProfileId: ownerProfileId
        );

        // Act
        var result = await _handler.Handle(command);

        // Assert
        Assert.Null(task.Description);
        Assert.Null(result.Description);

        Assert.Equal("Title", task.TitleValue);
        Assert.Equal(1, task.StatusId);

        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTaskDoesNotExist_ThrowsUzduotisNotFoundException()
    {
        // Arrange
        const int missingTaskId = 99;
        const int userProfileId = 10;

        _taskRepoMock
            .Setup(repo => repo.GetByIdForUpdateAsync(missingTaskId))
            .ReturnsAsync((Uzduotis?)null);

        var command = new UzduotisUpdateCommand(
            Id: missingTaskId,
            Title: Optional<string>.None(),
            Description: Optional<string>.None(),
            StatusId: Optional<int>.None(),
            UserProfileId: userProfileId
        );

        // Act
        var exception = await Assert.ThrowsAsync<UzduotisNotFoundException>(() =>
            _handler.Handle(command)
        );

        // Assert
        Assert.Equal($"Task with id {missingTaskId} was not found.", exception.Message);

        _taskRepoMock.Verify(repo => repo.GetByIdForUpdateAsync(missingTaskId), Times.Once);

        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserIsNotOwner_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        const int taskId = 99;
        const int actualOwnerProfileId = 20;
        const int requestingProfileId = 10;

        var task = CreateTask(
            id: taskId,
            userProfileId: actualOwnerProfileId,
            title: "Original title",
            description: "Original description",
            statusId: 1
        );

        _taskRepoMock.Setup(repo => repo.GetByIdForUpdateAsync(taskId)).ReturnsAsync(task);

        var command = new UzduotisUpdateCommand(
            Id: taskId,
            Title: Optional<string>.Some("Unauthorized change"),
            Description: Optional<string>.None(),
            StatusId: Optional<int>.None(),
            UserProfileId: requestingProfileId
        );

        // Act
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _handler.Handle(command)
        );

        // Assert
        Assert.Equal("You cannot edit this task.", exception.Message);

        // Entity must remain unchanged.
        Assert.Equal("Original title", task.TitleValue);
        Assert.Equal("Original description", task.Description);
        Assert.Equal(1, task.StatusId);
        Assert.Equal(actualOwnerProfileId, task.UserProfileId);

        _taskRepoMock.Verify(repo => repo.GetByIdForUpdateAsync(taskId), Times.Once);

        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTitleIsExplicitlyNull_ThrowsArgumentException()
    {
        // Arrange
        const int taskId = 99;
        const int ownerProfileId = 10;

        var task = CreateTask(
            id: taskId,
            userProfileId: ownerProfileId,
            title: "Original title",
            description: "Description",
            statusId: 1
        );

        _taskRepoMock.Setup(repo => repo.GetByIdForUpdateAsync(taskId)).ReturnsAsync(task);

        var command = new UzduotisUpdateCommand(
            Id: taskId,
            Title: Optional<string>.Some(null!),
            Description: Optional<string>.None(),
            StatusId: Optional<int>.None(),
            UserProfileId: ownerProfileId
        );

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command));

        // Assert
        Assert.Equal("Title cannot be null.", exception.Message);

        Assert.Equal("Original title", task.TitleValue);

        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }

    private static Uzduotis CreateTask(
        int id,
        int userProfileId,
        string title,
        string? description,
        int statusId,
        DateTime? createdAt = null,
        DateTime? updatedAt = null
    )
    {
        var task = new Uzduotis
        {
            Id = id,
            UserProfileId = userProfileId,
            Description = description,
            StatusId = statusId,
            CreatedAt = createdAt ?? DateTime.UtcNow.AddDays(-2),
        };

        task.SetTitle(title);

        /*
         * SetTitle() pakeičia UpdatedAt, todėl teste po SetTitle()
         * įrašome kontroliuojamą pradinę reikšmę.
         */
        task.UpdatedAt = updatedAt ?? DateTime.UtcNow.AddDays(-1);

        return task;
    }
}
