using Moq;
using TaskSystem.Application.Commands.Uzduotys.UzduotisCreate;
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
    public async Task Handle_WhenProfileExists_CreatesTaskAndReturnsMappedDto()
    {
        // Arrange
        const int expectedTaskId = 99;
        const int expectedProfileId = 10;
        const string expectedTitle = "Title";
        const string expectedDescription = "Description";
        const int expectedStatusId = 1;

        var command = new UzduotisCreateCommand(
            expectedTitle,
            expectedDescription,
            expectedProfileId
        );

        var profile = new UserProfile { Id = expectedProfileId };

        Uzduotis? createdTask = null;

        _profileRepoMock.Setup(repo => repo.GetByIdAsync(expectedProfileId)).ReturnsAsync(profile);

        _repoMock
            .Setup(repo => repo.AddAsync(It.IsAny<Uzduotis>()))
            .Callback<Uzduotis>(task =>
            {
                // Imituojame DB sugeneruojamą ID.
                task.Id = expectedTaskId;
                createdTask = task;
            })
            .Returns(Task.CompletedTask);

        _repoMock.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);

        var beforeExecution = DateTime.UtcNow;

        // Act
        var result = await _handler.Handle(command);

        var afterExecution = DateTime.UtcNow;

        // Assert: returned DTO
        Assert.Equal(expectedTaskId, result.Id);
        Assert.Equal(expectedTitle, result.Title);
        Assert.Equal(expectedDescription, result.Description);
        Assert.Equal(expectedStatusId, result.StatusId);
        Assert.Equal(expectedProfileId, result.UserProfileId);

        Assert.InRange(result.CreatedAt, beforeExecution, afterExecution);

        Assert.InRange(result.UpdatedAt, beforeExecution, afterExecution);

        // Assert: created entity
        Assert.NotNull(createdTask);

        Assert.Equal(expectedTaskId, createdTask.Id);
        Assert.Equal(expectedTitle, createdTask.TitleValue);
        Assert.Equal(expectedDescription, createdTask.Description);
        Assert.Equal(expectedStatusId, createdTask.StatusId);
        Assert.Equal(expectedProfileId, createdTask.UserProfileId);

        Assert.InRange(createdTask.CreatedAt, beforeExecution, afterExecution);

        Assert.InRange(createdTask.UpdatedAt, beforeExecution, afterExecution);

        Assert.True(createdTask.UpdatedAt >= createdTask.CreatedAt);

        // DTO turi būti sukurtas iš tos pačios entity būsenos.
        Assert.Equal(createdTask.CreatedAt, result.CreatedAt);
        Assert.Equal(createdTask.UpdatedAt, result.UpdatedAt);

        // Assert: dependency interactions
        _profileRepoMock.Verify(repo => repo.GetByIdAsync(expectedProfileId), Times.Once);

        _repoMock.Verify(
            repo =>
                repo.AddAsync(
                    It.Is<Uzduotis>(task =>
                        task.Id == expectedTaskId
                        && task.TitleValue == expectedTitle
                        && task.Description == expectedDescription
                        && task.StatusId == expectedStatusId
                        && task.UserProfileId == expectedProfileId
                    )
                ),
            Times.Once
        );

        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProfileDoesNotExist_ThrowsUserProfileNotFoundException()
    {
        // Arrange
        const int missingProfileId = 10;

        var command = new UzduotisCreateCommand("Title", "Description", missingProfileId);

        _profileRepoMock
            .Setup(repo => repo.GetByIdAsync(missingProfileId))
            .ReturnsAsync((UserProfile?)null);

        // Act
        var exception = await Assert.ThrowsAsync<UserProfileNotFoundException>(() =>
            _handler.Handle(command)
        );

        // Assert
        Assert.Equal($"User profile with id {missingProfileId} was not found.", exception.Message);

        _profileRepoMock.Verify(repo => repo.GetByIdAsync(missingProfileId), Times.Once);

        _repoMock.Verify(repo => repo.AddAsync(It.IsAny<Uzduotis>()), Times.Never);

        _repoMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
    }
}
