using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Operation> Operations => Set<Operation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(e =>
        {
            e.Property(u => u.Name).HasMaxLength(50);
            e.HasIndex(u => u.Name).IsUnique();
            e.Property(u => u.Balance).HasPrecision(18, 2);
        });

        builder.Entity<Operation>()
            .Property(o => o.Amount).HasPrecision(18, 2);
    }
}