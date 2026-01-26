using Microsoft.EntityFrameworkCore;
using Planara.Accounts.Data.Domain;

namespace Planara.Accounts.Data;

public class DataContext(DbContextOptions options): DbContext(options)
{
    public DbSet<Profile> Profiles { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Profile>()
            .HasKey(x => x.UserId);
        
        modelBuilder.Entity<Profile>()
            .HasIndex(x => x.UserId).IsUnique();
        
        modelBuilder.Entity<Profile>()
            .HasIndex(x => x.Username).IsUnique();
    }
}