using Microsoft.EntityFrameworkCore;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.ValueObjects;

namespace TaskSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Uzduotis> Uzduotys => Set<Uzduotis>();
    public DbSet<UzduotisStatus> UzduotisStatuses => Set<UzduotisStatus>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder
            .Entity<UzduotisStatus>()
            .HasData(
                new UzduotisStatus { Id = 1, Name = "New" },
                new UzduotisStatus { Id = 2, Name = "InProgress" },
                new UzduotisStatus { Id = 3, Name = "Done" }
            );
        modelBuilder
            .Entity<Uzduotis>()
            .Property<TaskTitle>("_title")
            .HasConversion(v => v.Value, v => TaskTitle.Create(v))
            .HasColumnName("Title");
        modelBuilder
            .Entity<User>()
            .Property<Email>("_email")
            .HasConversion(v => v.Value, v => Email.Create(v))
            .HasColumnName("Email");
    }
}
