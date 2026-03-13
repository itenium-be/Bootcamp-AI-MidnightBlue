using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Entities;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.Data;

public class AppDbContext : ForgeIdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TeamEntity> Teams => Set<TeamEntity>();

    public DbSet<CourseEntity> Courses => Set<CourseEntity>();

    public DbSet<ResourceEntity> Resources => Set<ResourceEntity>();

    public DbSet<ResourceCompletionEntity> ResourceCompletions => Set<ResourceCompletionEntity>();

    public DbSet<ResourceRatingEntity> ResourceRatings => Set<ResourceRatingEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ResourceCompletionEntity>()
            .HasIndex(c => new { c.ConsultantId, c.ResourceId })
            .IsUnique();

        builder.Entity<ResourceRatingEntity>()
            .HasIndex(r => new { r.ConsultantId, r.ResourceId })
            .IsUnique();
    }
}
