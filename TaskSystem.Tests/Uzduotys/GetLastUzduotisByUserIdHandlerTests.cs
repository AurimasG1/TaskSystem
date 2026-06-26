using Moq;
using TaskSystem.Application.DTO.Uzduotys;
using TaskSystem.Application.Queries.Uzduotys.GetLastUzduotisByUserId;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.Exceptions;
using TaskSystem.Domain.Interfaces;

namespace TaskSystem.Tests.Uzduotys;

public class GetLastUzduotisByUserIdHandlerTests
{
    private readonly Mock<IUzduotisRepository> _taskRepoMock;
    private readonly GetLastUzduotisByUserIdHandler _handler;

    public GetLastUzduotisByUserIdHandlerTests()
    {
        _taskRepoMock = new Mock<IUzduotisRepository>();
        _handler = new GetLastUzduotisByUserIdHandler(_taskRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsDto_WhenTaskExists()
    {
        // Arrange
        var task = new Uzduotis
        {
            Id = 10,
            Description = "Test Desc",
            StatusId = 1,
            UserId = 5,
            CreatedAt = DateTime.UtcNow.AddMinutes(-20),
            UpdatedAt = DateTime.UtcNow,
        };
        task.SetTitle("Test Title");

        _taskRepoMock.Setup(r => r.GetLastByUserIdAsync(5)).ReturnsAsync(task);

        var query = new GetLastUzduotisByUserIdQuery(UserId: 5);

        // Act
        UzduotisDto result = await _handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal("Test Title", result.Title);
        Assert.Equal("Test Desc", result.Description);
        Assert.Equal(1, result.StatusId);
        Assert.Equal(5, result.UserId);
    }

    [Fact]
    public async Task Handle_ThrowsNotFound_WhenNoTaskExists()
    {
        // Arrange
        _taskRepoMock.Setup(r => r.GetLastByUserIdAsync(5)).ReturnsAsync((Uzduotis?)null);

        var query = new GetLastUzduotisByUserIdQuery(UserId: 5);

        // Act + Assert
        await Assert.ThrowsAsync<UzduotisNotFoundException>(() => _handler.Handle(query));
    }
}
