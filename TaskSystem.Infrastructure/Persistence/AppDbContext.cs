using Microsoft.EntityFrameworkCore;
using TaskSystem.Domain.Entities;
using TaskSystem.Domain.ValueObjects;

namespace TaskSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Uzduotis> Uzduotys { get; set; }
    public DbSet<UzduotisStatus> UzduotisStatuses { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }

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
        modelBuilder
            .Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
