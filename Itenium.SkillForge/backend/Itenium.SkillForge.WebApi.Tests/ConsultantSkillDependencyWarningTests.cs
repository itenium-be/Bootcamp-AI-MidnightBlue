using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class ConsultantSkillDependencyWarningTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private ConsultantController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _user.IsBackOffice.Returns(true);
        _sut = new ConsultantController(Db, _user);
    }

    private async Task<(string userId, CompetenceCentreProfileEntity profile)> SeedConsultantWithProfile(
        string firstName = "Lea",
        string lastName = "Net")
    {
        var team = new TeamEntity { Name = ".NET" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        var profile = new CompetenceCentreProfileEntity { Name = "DotNet" };
        Db.CompetenceCentreProfiles.Add(profile);
        await Db.SaveChangesAsync();

        var userId = Guid.NewGuid().ToString();
        Db.Users.Add(new ForgeUser
        {
            Id = userId,
            UserName = $"{firstName}{lastName}".ToLowerInvariant(),
            Email = $"{firstName.ToLowerInvariant()}@test.local",
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
        });
        Db.ConsultantProfiles.Add(new ConsultantProfileEntity
        {
            UserId = userId,
            TeamId = team.Id,
            ProfileId = profile.Id,
        });
        await Db.SaveChangesAsync();

        return (userId, profile);
    }

    [Test]
    public async Task GetConsultantSkills_WhenSkillHasNoPrerequisites_ReturnsEmptyUnmetList()
    {
        var (userId, profile) = await SeedConsultantWithProfile();

        var skill = new SkillEntity { Name = "Clean Code", Category = "Craftsmanship", LevelCount = 3 };
        Db.Skills.Add(skill);
        await Db.SaveChangesAsync();

        Db.CompetenceCentreProfileSkills.Add(new CompetenceCentreProfileSkillEntity
        {
            ProfileId = profile.Id,
            SkillId = skill.Id,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetConsultantSkills(userId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var categories = ok!.Value as IReadOnlyList<RoadmapCategoryDto>;
        Assert.That(categories, Is.Not.Null);
        Assert.That(categories![0].Skills[0].UnmetPrerequisites, Is.Empty);
    }

    [Test]
    public async Task GetConsultantSkills_WhenPrerequisiteNotMet_ReturnsUnmetPrerequisite()
    {
        var (userId, profile) = await SeedConsultantWithProfile();

        var prereq = new SkillEntity { Name = "Clean Code", Category = "Craftsmanship", LevelCount = 3 };
        var skill = new SkillEntity { Name = "Domain-Driven Design", Category = "Architecture", LevelCount = 3 };
        Db.Skills.AddRange(prereq, skill);
        await Db.SaveChangesAsync();

        Db.SkillPrerequisites.Add(new SkillPrerequisiteEntity
        {
            SkillId = skill.Id,
            RequiredSkillId = prereq.Id,
            RequiredLevel = 3,
        });
        Db.CompetenceCentreProfileSkills.Add(new CompetenceCentreProfileSkillEntity
        {
            ProfileId = profile.Id,
            SkillId = skill.Id,
        });
        await Db.SaveChangesAsync();

        // Consultant has no skill levels recorded — prerequisite is unmet

        var result = await _sut.GetConsultantSkills(userId);

        var ok = result.Result as OkObjectResult;
        var categories = ok!.Value as IReadOnlyList<RoadmapCategoryDto>;
        var ddd = categories![0].Skills.First(s => s.Name == "Domain-Driven Design");
        Assert.That(ddd.UnmetPrerequisites, Has.Count.EqualTo(1));
        Assert.That(ddd.UnmetPrerequisites[0].RequiredSkillId, Is.EqualTo(prereq.Id));
        Assert.That(ddd.UnmetPrerequisites[0].RequiredSkillName, Is.EqualTo("Clean Code"));
        Assert.That(ddd.UnmetPrerequisites[0].RequiredLevel, Is.EqualTo(3));
    }

    [Test]
    public async Task GetConsultantSkills_WhenPrerequisiteMetExactly_ReturnsNoWarning()
    {
        var (userId, profile) = await SeedConsultantWithProfile();

        var prereq = new SkillEntity { Name = "Clean Code", Category = "Craftsmanship", LevelCount = 3 };
        var skill = new SkillEntity { Name = "Domain-Driven Design", Category = "Architecture", LevelCount = 3 };
        Db.Skills.AddRange(prereq, skill);
        await Db.SaveChangesAsync();

        Db.SkillPrerequisites.Add(new SkillPrerequisiteEntity
        {
            SkillId = skill.Id,
            RequiredSkillId = prereq.Id,
            RequiredLevel = 3,
        });
        Db.CompetenceCentreProfileSkills.Add(new CompetenceCentreProfileSkillEntity
        {
            ProfileId = profile.Id,
            SkillId = skill.Id,
        });
        Db.ConsultantSkillLevels.Add(new ConsultantSkillLevelEntity
        {
            UserId = userId,
            SkillId = prereq.Id,
            CurrentLevel = 3,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetConsultantSkills(userId);

        var ok = result.Result as OkObjectResult;
        var categories = ok!.Value as IReadOnlyList<RoadmapCategoryDto>;
        var ddd = categories![0].Skills.First(s => s.Name == "Domain-Driven Design");
        Assert.That(ddd.UnmetPrerequisites, Is.Empty);
    }

    [Test]
    public async Task GetConsultantSkills_WhenPrerequisitePartiallyMet_ReturnsWarning()
    {
        var (userId, profile) = await SeedConsultantWithProfile();

        var prereq = new SkillEntity { Name = "Clean Code", Category = "Craftsmanship", LevelCount = 3 };
        var skill = new SkillEntity { Name = "Domain-Driven Design", Category = "Architecture", LevelCount = 3 };
        Db.Skills.AddRange(prereq, skill);
        await Db.SaveChangesAsync();

        Db.SkillPrerequisites.Add(new SkillPrerequisiteEntity
        {
            SkillId = skill.Id,
            RequiredSkillId = prereq.Id,
            RequiredLevel = 3,
        });
        Db.CompetenceCentreProfileSkills.Add(new CompetenceCentreProfileSkillEntity
        {
            ProfileId = profile.Id,
            SkillId = skill.Id,
        });
        // Consultant has level 2 but needs level 3
        Db.ConsultantSkillLevels.Add(new ConsultantSkillLevelEntity
        {
            UserId = userId,
            SkillId = prereq.Id,
            CurrentLevel = 2,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetConsultantSkills(userId);

        var ok = result.Result as OkObjectResult;
        var categories = ok!.Value as IReadOnlyList<RoadmapCategoryDto>;
        var ddd = categories![0].Skills.First(s => s.Name == "Domain-Driven Design");
        Assert.That(ddd.UnmetPrerequisites, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetConsultantSkills_WhenPrerequisiteExceeded_ReturnsNoWarning()
    {
        var (userId, profile) = await SeedConsultantWithProfile();

        var prereq = new SkillEntity { Name = "Clean Code", Category = "Craftsmanship", LevelCount = 3 };
        var skill = new SkillEntity { Name = "Domain-Driven Design", Category = "Architecture", LevelCount = 3 };
        Db.Skills.AddRange(prereq, skill);
        await Db.SaveChangesAsync();

        Db.SkillPrerequisites.Add(new SkillPrerequisiteEntity
        {
            SkillId = skill.Id,
            RequiredSkillId = prereq.Id,
            RequiredLevel = 2,
        });
        Db.CompetenceCentreProfileSkills.Add(new CompetenceCentreProfileSkillEntity
        {
            ProfileId = profile.Id,
            SkillId = skill.Id,
        });
        // Consultant has level 3, needs level 2 — exceeded, no warning
        Db.ConsultantSkillLevels.Add(new ConsultantSkillLevelEntity
        {
            UserId = userId,
            SkillId = prereq.Id,
            CurrentLevel = 3,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetConsultantSkills(userId);

        var ok = result.Result as OkObjectResult;
        var categories = ok!.Value as IReadOnlyList<RoadmapCategoryDto>;
        var ddd = categories![0].Skills.First(s => s.Name == "Domain-Driven Design");
        Assert.That(ddd.UnmetPrerequisites, Is.Empty);
    }

    [Test]
    public async Task GetConsultantSkills_ReturnsRoadmapSkillDto()
    {
        var (userId, profile) = await SeedConsultantWithProfile();

        var skill = new SkillEntity { Name = "Clean Code", Category = "Craftsmanship", LevelCount = 3 };
        Db.Skills.Add(skill);
        await Db.SaveChangesAsync();

        Db.CompetenceCentreProfileSkills.Add(new CompetenceCentreProfileSkillEntity
        {
            ProfileId = profile.Id,
            SkillId = skill.Id,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetConsultantSkills(userId);

        var ok = result.Result as OkObjectResult;
        var categories = ok!.Value as IReadOnlyList<RoadmapCategoryDto>;
        var roadmapSkill = categories![0].Skills[0];
        Assert.That(roadmapSkill.Id, Is.EqualTo(skill.Id));
        Assert.That(roadmapSkill.Name, Is.EqualTo("Clean Code"));
        Assert.That(roadmapSkill.LevelCount, Is.EqualTo(3));
    }
}