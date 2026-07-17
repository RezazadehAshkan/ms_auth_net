using Microsoft.EntityFrameworkCore;
using AuthenticationService.Domain.Entities;

namespace AuthenticationService.Infrastructure.Persistence;

public class AuthenticationDbContext : DbContext
{
    public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RefreshToken>(entity =>
        {
        entity.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(x => x.UserId);

        entity.HasIndex(x => x.TokenHash)
            .IsUnique();
        });
    }
}
