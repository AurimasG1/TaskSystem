using Moq;
using TaskSystem.Application.DTO.Responses.Uzduotys;
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
    public async Task Handle_ReturnsLastTask()
    {
        var task = new Uzduotis
        {
            Id = 99,
            UserProfileId = 10,
            Description = "Desc",
            StatusId = 2,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1),
        };
        task.SetTitle("Title");

        _repoMock.Setup(r => r.GetLastByUserProfileIdAsync(10)).ReturnsAsync(task);

        var query = new GetLastUzduotisByUserProfileIdQuery(10);

        UzduotisDto result = await _handler.Handle(query);

        Assert.Equal(99, result.Id);
        Assert.Equal("Title", result.Title);
    }

    [Fact]
    public async Task Handle_Throws_WhenNoTasks()
    {
        _repoMock.Setup(r => r.GetLastByUserProfileIdAsync(10)).ReturnsAsync((Uzduotis?)null);

        var query = new GetLastUzduotisByUserProfileIdQuery(10);

        await Assert.ThrowsAsync<UzduotisNotFoundException>(() => _handler.Handle(query));
    }
}
