using Moq;
using TaskSystem.Common.DTO;
using TaskSystem.Entities;
using TaskSystem.Repositories.Interface;
using TaskSystem.Services.Implementation;

namespace TaskSystem.Tests;

public class UzduotisServiceTests
{
    private UzduotisService CreateService(Mock<IUzduotisRepository> repoMock)
    {
        return new UzduotisService(repoMock.Object);
    }

    // ------------------------------------------------------------
    // TESTAS 1:
    // Tikrina, ar servisas teisingai grąžina paskutinę naudotojo
    // užduotį, kai repository ją randa.
    // ------------------------------------------------------------
    [Fact]
    public async Task GetLastByUserIdAsync_ReturnsDto_WhenUzduotisExists()
    {
        var repoMock = new Mock<IUzduotisRepository>();
        var service = CreateService(repoMock);

        int userId = 1;

        var uzduotis = new Uzduotis
        {
            Id = 10,
            Title = "Test Task",
            Description = "Test Desc",
            Status = new UzduotisStatus { Id = 1, Name = "New" },
            StatusId = 1,
            UserId = userId,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            UpdatedAt = DateTime.UtcNow,
        };

        repoMock.Setup(r => r.GetLastByUserIdAsync(userId)).ReturnsAsync(uzduotis);

        var result = await service.GetLastByUserIdAsync(userId);

        Assert.NotNull(result);
        Assert.Equal(10, result!.Id);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("New", result.Status);
    }

    // ------------------------------------------------------------
    // TESTAS 2:
    // Tikrina, ar servisas meta ArgumentException, kai Title tuščias.
    // ------------------------------------------------------------
    [Fact]
    public async Task CreateAsync_ThrowsException_WhenTitleIsMissing()
    {
        var repoMock = new Mock<IUzduotisRepository>();
        var service = CreateService(repoMock);

        var request = new UzduotisRequestDto
        {
            Title = "",
            Description = "Test",
            StatusId = 1,
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(request, userId: 1));
    }

    // ------------------------------------------------------------
    // TESTAS 3:
    // Tikrina, ar servisas atnaujina UpdatedAt lauką.
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateAsync_UpdatesUpdatedAt_WhenUzduotisIsModified()
    {
        var repoMock = new Mock<IUzduotisRepository>();
        var service = CreateService(repoMock);

        var uzduotis = new Uzduotis
        {
            Id = 1,
            Title = "Old Title",
            Description = "Old",
            StatusId = 1,
            UserId = 1,
            CreatedAt = DateTime.UtcNow.AddMinutes(-20),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-20),
        };

        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(uzduotis);

        var request = new UzduotisUpdateRequestDto
        {
            Title = "New Title",
            Description = "Updated",
            StatusId = 1,
        };

        await service.UpdateAsync(1, request, userId: 1);

        Assert.True(uzduotis.UpdatedAt > uzduotis.CreatedAt);
    }

    // ------------------------------------------------------------
    // TESTAS 4:
    // Tikrina, ar servisas iškviečia repository metodą ResetLastUzduotisAsync.
    // ------------------------------------------------------------
    [Fact]
    public async Task ResetLastUzduotisAsync_UpdatesEntityAndSaves()
    {
        var repoMock = new Mock<IUzduotisRepository>();
        var service = new UzduotisService(repoMock.Object);

        var uzduotis = new Uzduotis
        {
            Id = 1,
            Title = "Old",
            Description = "Old desc",
            StatusId = 5,
            UserId = 1,
            UpdatedAt = DateTime.UtcNow.AddMinutes(-10),
        };

        repoMock.Setup(r => r.GetLastByUserIdAsync(1)).ReturnsAsync(uzduotis);

        var result = await service.ResetLastUzduotisAsync(1);

        Assert.True(result);
        Assert.Equal("(reset) Old", uzduotis.Title);
        Assert.Null(uzduotis.Description);
        Assert.Equal(1, uzduotis.StatusId);

        repoMock.Verify(r => r.UpdateAsync(uzduotis), Times.Once);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
