using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InfraSizingCalculator.Data.Identity;

/// <summary>
/// Database context for Identity authentication
/// Uses a separate SQLite database for user management
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize Identity tables
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
        });

        // Seed default admin user (password will be set on first run)
        SeedDefaultRoles(builder);
    }

    private static void SeedDefaultRoles(ModelBuilder builder)
    {
        // Seed default roles
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "1",
                Name = "Admin",
                NormalizedName = "ADMIN"
            },
            new IdentityRole
            {
                Id = "2",
                Name = "User",
                NormalizedName = "USER"
            },
            new IdentityRole
            {
                Id = "3",
                Name = "Viewer",
                NormalizedName = "VIEWER"
            }
        );
    }
}
