using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class ConsultantRoadmapDefaultViewTests : DatabaseTestBase
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

    private async Task<(string userId, CompetenceCentreProfileEntity profile)> SeedConsultantWithProfile()
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
            UserName = $"user{userId[..8]}",
            Email = $"user{userId[..8]}@test.local",
            EmailConfirmed = true,
            FirstName = "Test",
            LastName = "User",
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

    private async Task AddSkillToProfile(int profileId, SkillEntity skill)
    {
        Db.CompetenceCentreProfileSkills.Add(new CompetenceCentreProfileSkillEntity
        {
            ProfileId = profileId,
            SkillId = skill.Id,
        });
        await Db.SaveChangesAsync();
    }

    private IReadOnlyList<RoadmapSkillDto> GetSkills(ActionResult<IReadOnlyList<RoadmapCategoryDto>> result)
    {
        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var categories = ok!.Value as IReadOnlyList<RoadmapCategoryDto>;
        Assert.That(categories, Is.Not.Null);
        return categories!.SelectMany(c => c.Skills).ToList();
    }

    [Test]
    public async Task GetConsultantSkills_SkillWithCurrentLevel_IsInDefaultView()
    {
        var (userId, profile) = await SeedConsultantWithProfile();

        var skill = new SkillEntity { Name = "Clean Code", Category = "Craftsmanship", LevelCount = 3 };
        Db.Skills.Add(skill);
        await Db.SaveChangesAsync();
        await AddSkillToProfile(profile.Id, skill);

        Db.ConsultantSkillLevels.Add(new ConsultantSkillLevelEntity
        {
            UserId = userId,
            SkillId = skill.Id,
            CurrentLevel = 1,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetConsultantSkills(userId);

        var skills = GetSkills(result);
        Assert.That(skills.Single(s => s.Name == "Clean Code").IsInDefaultView, Is.True);
    }

    [Test]
    public async Task GetConsultantSkills_SkillWithNoPrerequisites_IsInDefaultView()
    {
        var (userId, profile) = await SeedConsultantWithProfile();

        var skill = new SkillEntity { Name = "Clean Code", Category = "Craftsmanship", LevelCount = 3 };
        Db.Skills.Add(skill);
        await Db.SaveChangesAsync();
        await AddSkillToProfile(profile.Id, skill);

        var result = await _sut.GetConsultantSkills(userId);

        var skills = GetSkills(result);
        Assert.That(skills.Single(s => s.Name == "Clean Code").IsInDefaultView, Is.True);
    }

    [Test]
    public async Task GetConsultantSkills_SkillWithUnmetPrerequisites_IsNotInDefaultView()
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
        await Db.SaveChangesAsync();
        await AddSkillToProfile(profile.Id, skill);

        // Consultant has no skill levels — prerequisite is unmet
        // But with only 1 skill total, padding kicks in — ensure we test with enough skills

        // Add 8 more skills with no prereqs so padding fills default view without reaching DDD
        for (var i = 1; i <= 8; i++)
        {
            var pad = new SkillEntity { Name = $"Pad Skill {i}", Category = "Other", LevelCount = 1 };
            Db.Skills.Add(pad);
            await Db.SaveChangesAsync();
            await AddSkillToProfile(profile.Id, pad);
        }

        var result = await _sut.GetConsultantSkills(userId);

        var skills = GetSkills(result);
        Assert.That(skills.Single(s => s.Name == "Domain-Driven Design").IsInDefaultView, Is.False);
    }

    [Test]
    public async Task GetConsultantSkills_SkillWithAllPrerequisitesMet_IsInDefaultView()
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
        await Db.SaveChangesAsync();
        await AddSkillToProfile(profile.Id, skill);

        Db.ConsultantSkillLevels.Add(new ConsultantSkillLevelEntity
        {
            UserId = userId,
            SkillId = prereq.Id,
            CurrentLevel = 2,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetConsultantSkills(userId);

        var skills = GetSkills(result);
        Assert.That(skills.Single(s => s.Name == "Domain-Driven Design").IsInDefaultView, Is.True);
    }

    [Test]
    public async Task GetConsultantSkills_WhenFewerThan8DefaultSkills_PadsWithMostAccessible()
    {
        var (userId, profile) = await SeedConsultantWithProfile();

        // 1 skill with no prereqs (next-tier)
        var easy = new SkillEntity { Name = "Easy Skill", Category = "Easy", LevelCount = 1 };
        Db.Skills.Add(easy);
        await Db.SaveChangesAsync();
        await AddSkillToProfile(profile.Id, easy);

        // 10 skills each with 1 unmet prereq (eligible for padding, ordered by name)
        var prereq = new SkillEntity { Name = "Base Skill", Category = "Easy", LevelCount = 1 };
        Db.Skills.Add(prereq);
        await Db.SaveChangesAsync();

        for (var i = 1; i <= 10; i++)
        {
            var padSkill = new SkillEntity { Name = $"Hard Skill {i:D2}", Category = "Hard", LevelCount = 1 };
            Db.Skills.Add(padSkill);
            await Db.SaveChangesAsync();
            Db.SkillPrerequisites.Add(new SkillPrerequisiteEntity
            {
                SkillId = padSkill.Id,
                RequiredSkillId = prereq.Id,
                RequiredLevel = 1,
            });
            await Db.SaveChangesAsync();
            await AddSkillToProfile(profile.Id, padSkill);
        }

        var result = await _sut.GetConsultantSkills(userId);

        var skills = GetSkills(result);
        var defaultCount = skills.Count(s => s.IsInDefaultView);
        var nonDefaultCount = skills.Count(s => !s.IsInDefaultView);

        // 1 next-tier + 7 padded = 8 in default view, 3 not
        Assert.That(defaultCount, Is.EqualTo(8));
        Assert.That(nonDefaultCount, Is.EqualTo(3));
    }

    [Test]
    public async Task GetConsultantSkills_WhenAllSkillsUnlocked_AllAreInDefaultView()
    {
        var (userId, profile) = await SeedConsultantWithProfile();

        for (var i = 1; i <= 12; i++)
        {
            var skill = new SkillEntity { Name = $"Skill {i}", Category = "Cat", LevelCount = 1 };
            Db.Skills.Add(skill);
            await Db.SaveChangesAsync();
            await AddSkillToProfile(profile.Id, skill);
        }

        var result = await _sut.GetConsultantSkills(userId);

        var skills = GetSkills(result);
        Assert.That(skills.All(s => s.IsInDefaultView), Is.True);
    }
}