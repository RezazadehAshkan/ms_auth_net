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
}
