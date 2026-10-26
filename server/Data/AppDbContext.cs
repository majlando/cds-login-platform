using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Data;

// Connects our C# code to the SQLite database, using Entity Framework Core.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // The Users table
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Two users can't have the same username
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
    }
}
