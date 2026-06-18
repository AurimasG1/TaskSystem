using Microsoft.EntityFrameworkCore;
using Moq;
using TaskSystem.Common.DTO;
using TaskSystem.Data;
using TaskSystem.Entities;
using TaskSystem.Repositories.Interface;
using TaskSystem.Services.Implementation;

namespace TaskSystem.Tests;

public class UzduotisServiceTests
{
    // Sukuria unikalią InMemory duomenų bazę kiekvienam testui,
    // kad testai būtų izoliuoti ir nesidalintų duomenimis.
    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    // Sukuria UzduotisService su netikru (mock) repository ir InMemory DB.
    private UzduotisService CreateService(AppDbContext db, Mock<IUzduotisRepository> repoMock)
    {
        return new UzduotisService(repoMock.Object, db);
    }

    // ------------------------------------------------------------
    // TESTAS 1:
    // Tikrina, ar servisas teisingai grąžina paskutinę naudotojo
    // užduotį, kai repository ją randa.
    //
    // Tai tikrina verslo logiką: entity → DTO konversiją ir
    // teisingą duomenų grąžinimą.
    // ------------------------------------------------------------
    [Fact]
    public async Task GetLastByUserIdAsync_ReturnsDto_WhenUzduotisExists()
    {
        var db = CreateDbContext();
        var repoMock = new Mock<IUzduotisRepository>();
        var service = CreateService(db, repoMock);

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
    // Tikrina, ar servisas meta ArgumentException, kai bandoma
    // sukurti užduotį be Title.
    //
    // Tai tikrina verslo taisyklę: Title yra privalomas laukas.
    // ------------------------------------------------------------
    [Fact]
    public async Task CreateAsync_ThrowsException_WhenTitleIsMissing()
    {
        var db = CreateDbContext();
        var repoMock = new Mock<IUzduotisRepository>();
        var service = CreateService(db, repoMock);

        var request = new UzduotisRequestDto
        {
            Title = "", // neteisingas Title
            Description = "Test",
            StatusId = 1,
        };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateAsync(request, userId: 1));
    }

    // ------------------------------------------------------------
    // TESTAS 3:
    // Tikrina, ar servisas atnaujina UpdatedAt lauką, kai
    // užduotis yra modifikuojama.
    //
    // Tai tikrina laiko logiką: UpdatedAt turi būti naujesnis
    // už CreatedAt po atnaujinimo.
    // ------------------------------------------------------------
    [Fact]
    public async Task UpdateAsync_UpdatesUpdatedAt_WhenUzduotisIsModified()
    {
        var db = CreateDbContext();
        var repoMock = new Mock<IUzduotisRepository>();
        var service = CreateService(db, repoMock);

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

        db.Uzduotys.Add(uzduotis);
        db.SaveChanges();

        var request = new UzduotisUpdateRequestDto
        {
            Title = "New Title",
            Description = "Updated",
            StatusId = 1,
        };

        await service.UpdateAsync(1, request, userId: 1);

        var updated = db.Uzduotys.First(u => u.Id == 1);

        Assert.True(updated.UpdatedAt > uzduotis.CreatedAt);
    }

    // ------------------------------------------------------------
    // TESTAS 4:
    // Tikrina, ar servisas iškviečia repository metodą
    // ResetLastUzduotisAsync.
    //
    // Tai tikrina, ar servisas teisingai deleguoja darbą į repo.
    // ------------------------------------------------------------
    [Fact]
    public async Task ResetLastUzduotisAsync_CallsRepositoryMethod()
    {
        var db = CreateDbContext();
        var repoMock = new Mock<IUzduotisRepository>();
        var service = CreateService(db, repoMock);

        repoMock.Setup(r => r.ResetLastUzduotisAsync(1)).ReturnsAsync(true);

        var result = await service.ResetLastUzduotisAsync(1);

        Assert.True(result);
        repoMock.Verify(r => r.ResetLastUzduotisAsync(1), Times.Once);
    }
}
