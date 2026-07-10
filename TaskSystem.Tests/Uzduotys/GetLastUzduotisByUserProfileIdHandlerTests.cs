using Moq;
using TaskSystem.Application.Queries.Uzduotys.GetLastUzduotisByUserProfileId;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Uzduotys;

public class GetLastUzduotisByUserProfileIdHandlerTests
{
    private readonly Mock<IUzduotisRepository> _repoMock;
    private readonly GetLastUzduotisByUserProfileIdHandler _handler;

    public GetLastUzduotisByUserProfileIdHandlerTests()
    {
        _repoMock = new Mock<IUzduotisRepository>();

        _handler = new GetLastUzduotisByUserProfileIdHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_WhenTaskExists_ReturnsMappedTask()
    {
        // Arrange
        const int expectedTaskId = 99;
        const int expectedUserProfileId = 10;
        const string expectedTitle = "Title";
        const string expectedDescription = "Description";
        const int expectedStatusId = 2;

        var expectedCreatedAt = new DateTime(2026, 7, 1, 10, 0, 0, DateTimeKind.Utc);

        var expectedUpdatedAt = new DateTime(2026, 7, 2, 12, 30, 0, DateTimeKind.Utc);

        var uzduotis = new Uzduotis
        {
            Id = expectedTaskId,
            UserProfileId = expectedUserProfileId,
            Description = expectedDescription,
            StatusId = expectedStatusId,
            CreatedAt = expectedCreatedAt,
        };

        uzduotis.SetTitle(expectedTitle);
        uzduotis.UpdatedAt = expectedUpdatedAt;

        _repoMock
            .Setup(repo => repo.GetLastByUserProfileIdAsync(expectedUserProfileId))
            .ReturnsAsync(uzduotis);

        var query = new GetLastUzduotisByUserProfileIdQuery(expectedUserProfileId);

        // Act
        var result = await _handler.Handle(query);

        // Assert: returned DTO
        Assert.Equal(expectedTaskId, result.Id);
        Assert.Equal(expectedTitle, result.Title);
        Assert.Equal(expectedDescription, result.Description);
        Assert.Equal(expectedStatusId, result.StatusId);
        Assert.Equal(expectedUserProfileId, result.UserProfileId);
        Assert.Equal(expectedCreatedAt, result.CreatedAt);
        Assert.Equal(expectedUpdatedAt, result.UpdatedAt);

        // Assert: repository interaction
        _repoMock.Verify(
            repo => repo.GetLastByUserProfileIdAsync(expectedUserProfileId),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenTaskDoesNotExist_ThrowsUzduotisNotFoundException()
    {
        // Arrange
        const int userProfileId = 10;

        _repoMock
            .Setup(repo => repo.GetLastByUserProfileIdAsync(userProfileId))
            .ReturnsAsync((Uzduotis?)null);

        var query = new GetLastUzduotisByUserProfileIdQuery(userProfileId);

        // Act
        var exception = await Assert.ThrowsAsync<UzduotisNotFoundException>(() =>
            _handler.Handle(query)
        );

        // Assert
        Assert.Equal($"User {userProfileId} has no tasks.", exception.Message);

        _repoMock.Verify(repo => repo.GetLastByUserProfileIdAsync(userProfileId), Times.Once);
    }
}
