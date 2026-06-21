using Moq;
using TaskSystem.Common.DTO;
using TaskSystem.Entities;
using TaskSystem.Repositories.Interface;
using TaskSystem.Services.Implementation;

namespace TaskSystem.Tests;

public class UzduotisServiceTests
{
    private UzduotisService CreateService(Mock<IUzduotisRepository> repoMock) =>
        new UzduotisService(repoMock.Object);

    // ------------------------------------------------------------
    // GET LAST
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
            StatusId = 1,
            Status = new UzduotisStatus { Id = 1, Name = "New" },
            UserId = userId,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            UpdatedAt = DateTime.UtcNow,
        };

        repoMock.Setup(r => r.GetLastByUserIdAsync(userId)).ReturnsAsync(uzduotis);

        var result = await service.GetLastByUserIdAsync(userId);

        Assert.NotNull(result);
        Assert.Equal(10, result.Id);
        Assert.Equal("Test Task", result.Title);
        Assert.Equal("New", result.Status);
    }

    // ------------------------------------------------------------
    // CREATE — validacija vyksta validatoriuose, ne servise
    // ------------------------------------------------------------
    [Fact]
    public async Task CreateAsync_CreatesTask_AndReturnsDto()
    {
        var repoMock = new Mock<IUzduotisRepository>();
        var service = CreateService(repoMock);

        var request = new UzduotisRequestDto
        {
            Title = "New Task",
            Description = "Desc",
            StatusId = 1,
        };

        var savedEntity = new Uzduotis
        {
            Id = 1,
            Title = request.Title,
            Description = request.Description,
            StatusId = request.StatusId,
            Status = new UzduotisStatus { Id = 1, Name = "New" },
            UserId = 5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        // 🔥 svarbiausia vieta — nustatome Id
        repoMock.Setup(r => r.AddAsync(It.IsAny<Uzduotis>())).Callback<Uzduotis>(u => u.Id = 1);

        repoMock.Setup(r => r.SaveChangesAsync());
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(savedEntity);

        var result = await service.CreateAsync(request, 5);

        Assert.NotNull(result);
        Assert.Equal("New Task", result.Title);
        Assert.Equal("New", result.Status);

        repoMock.Verify(r => r.AddAsync(It.IsAny<Uzduotis>()), Times.Once);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ------------------------------------------------------------
    // UPDATE
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateAsync_UpdatesEntity_AndReturnsUpdatedDto()
    {
        var repoMock = new Mock<IUzduotisRepository>();
        var service = CreateService(repoMock);

        var original = new Uzduotis
        {
            Id = 1,
            Title = "Old",
            Description = "Old Desc",
            StatusId = 1,
            UserId = 1,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-30),
        };

        var updatedEntity = new Uzduotis
        {
            Id = 1,
            Title = "New",
            Description = "Updated",
            StatusId = 2,
            Status = new UzduotisStatus { Id = 2, Name = "In Progress" },
            UserId = 1,
            CreatedAt = original.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
        };

        repoMock.Setup(r => r.GetByIdForUpdateAsync(1)).ReturnsAsync(original);
        repoMock.Setup(r => r.UpdateAsync(original));
        repoMock.Setup(r => r.SaveChangesAsync());
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(updatedEntity);

        var request = new UzduotisUpdateRequestDto
        {
            Title = "New",
            Description = "Updated",
            StatusId = 2,
        };

        var result = await service.UpdateAsync(1, request, 1);

        Assert.NotNull(result);
        Assert.Equal("New", result.Title);
        Assert.Equal("In Progress", result.Status);

        repoMock.Verify(r => r.UpdateAsync(original), Times.Once);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // ------------------------------------------------------------
    // RESET LAST
    // ------------------------------------------------------------
    [Fact]
    public async Task ResetLastUzduotisAsync_ResetsFields_AndReturnsUpdatedDto()
    {
        var repoMock = new Mock<IUzduotisRepository>();
        var service = CreateService(repoMock);

        var original = new Uzduotis
        {
            Id = 1,
            Title = "Old",
            Description = "Old Desc",
            StatusId = 5,
            Status = new UzduotisStatus { Id = 5, Name = "OldStatus" },
            UserId = 1,
            UpdatedAt = DateTime.UtcNow.AddMinutes(-20),
        };

        var updatedEntity = new Uzduotis
        {
            Id = 1,
            Title = "(reset) Old",
            Description = null,
            StatusId = 1,
            Status = new UzduotisStatus { Id = 1, Name = "New" },
            UserId = 1,
            UpdatedAt = DateTime.UtcNow,
        };

        repoMock.Setup(r => r.GetLastByUserIdAsync(1)).ReturnsAsync(original);
        repoMock.Setup(r => r.UpdateAsync(original));
        repoMock.Setup(r => r.SaveChangesAsync());
        repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(updatedEntity);

        var result = await service.ResetLastUzduotisAsync(1);

        Assert.NotNull(result);
        Assert.Equal("(reset) Old", result.Title);
        Assert.Equal("New", result.Status);

        repoMock.Verify(r => r.UpdateAsync(original), Times.Once);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
