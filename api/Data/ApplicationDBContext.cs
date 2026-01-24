using api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace api.Data;

public class ApplicationDBContext : IdentityDbContext<AppUser>
{
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
    {
    }

    public DbSet<Track> Tracks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User-Track relationship
        modelBuilder.Entity<AppUser>()
            .HasMany(u => u.UploadedTracks)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        List<IdentityRole> initialRoles = new List<IdentityRole>
        {
            new IdentityRole
            {
                Id = "5b2aaae0-bace-408f-869b-0f773b0adbd0",
                Name = "User",
                NormalizedName = "USER"
            },
            new IdentityRole
            {
                Id = "d390c202-0f2f-4ece-8c77-3a9bfd4fcd45",
                Name = "Admin",
                NormalizedName = "ADMIN"
            }
        };
        modelBuilder.Entity<IdentityRole>().HasData(initialRoles);
    }
}