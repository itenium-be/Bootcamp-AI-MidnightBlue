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

    public DbSet<SkillEntity> Skills => Set<SkillEntity>();

    public DbSet<SkillLevelDescriptorEntity> SkillLevelDescriptors => Set<SkillLevelDescriptorEntity>();

    public DbSet<SkillPrerequisiteEntity> SkillPrerequisites => Set<SkillPrerequisiteEntity>();

    public DbSet<CompetenceCentreProfileEntity> CompetenceCentreProfiles => Set<CompetenceCentreProfileEntity>();

    public DbSet<CompetenceCentreProfileSkillEntity> CompetenceCentreProfileSkills => Set<CompetenceCentreProfileSkillEntity>();

    public DbSet<ConsultantProfileEntity> ConsultantProfiles => Set<ConsultantProfileEntity>();

    public DbSet<ConsultantSkillLevelEntity> ConsultantSkillLevels => Set<ConsultantSkillLevelEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SkillPrerequisiteEntity>(e =>
        {
            e.HasKey(x => new { x.SkillId, x.RequiredSkillId });

            e.HasOne(x => x.Skill)
                .WithMany(x => x.Prerequisites)
                .HasForeignKey(x => x.SkillId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.RequiredSkill)
                .WithMany(x => x.Dependents)
                .HasForeignKey(x => x.RequiredSkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CompetenceCentreProfileSkillEntity>(e =>
        {
            e.HasKey(x => new { x.ProfileId, x.SkillId });

            e.HasOne(x => x.Profile)
                .WithMany(x => x.ProfileSkills)
                .HasForeignKey(x => x.ProfileId);

            e.HasOne(x => x.Skill)
                .WithMany(x => x.ProfileSkills)
                .HasForeignKey(x => x.SkillId);
        });

        builder.Entity<ConsultantProfileEntity>(e =>
        {
            e.HasOne(x => x.Profile)
                .WithMany()
                .HasForeignKey(x => x.ProfileId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });

        builder.Entity<ConsultantSkillLevelEntity>(e =>
        {
            e.HasKey(x => new { x.UserId, x.SkillId });

            e.HasOne(x => x.Skill)
                .WithMany()
                .HasForeignKey(x => x.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
