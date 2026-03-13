using Itenium.SkillForge.Entities;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class CompetenceCentreProfileControllerTests : DatabaseTestBase
{
    private CompetenceCentreProfileController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _sut = new CompetenceCentreProfileController(Db);
    }

    [Test]
    public async Task GetProfiles_ReturnsAllProfiles()
    {
        Db.CompetenceCentreProfiles.AddRange(
            new CompetenceCentreProfileEntity { Name = ".NET" },
            new CompetenceCentreProfileEntity { Name = "Java" });
        await Db.SaveChangesAsync();

        var result = await _sut.GetProfiles();

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var profiles = ok!.Value as List<CompetenceCentreProfileSummaryDto>;
        Assert.That(profiles, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetProfileSkills_ReturnsSkillsForProfile()
    {
        var skill1 = new SkillEntity { Name = "C# Language", Category = ".NET", LevelCount = 5 };
        var skill2 = new SkillEntity { Name = "ASP.NET Core", Category = ".NET", LevelCount = 4 };
        var otherSkill = new SkillEntity { Name = "Spring Boot", Category = "Java", LevelCount = 4 };
        Db.Skills.AddRange(skill1, skill2, otherSkill);

        var profile = new CompetenceCentreProfileEntity { Name = ".NET" };
        Db.CompetenceCentreProfiles.Add(profile);
        await Db.SaveChangesAsync();

        Db.CompetenceCentreProfileSkills.AddRange(
            new CompetenceCentreProfileSkillEntity { ProfileId = profile.Id, SkillId = skill1.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = profile.Id, SkillId = skill2.Id });
        await Db.SaveChangesAsync();

        var result = await _sut.GetProfileSkills(profile.Id);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var skills = ok!.Value as List<SkillSummaryDto>;
        Assert.That(skills, Has.Count.EqualTo(2));
        Assert.That(skills!.Select(s => s.Name), Does.Not.Contain("Spring Boot"));
    }

    [Test]
    public async Task GetProfileSkills_WhenProfileNotFound_ReturnsNotFound()
    {
        var result = await _sut.GetProfileSkills(999);
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetProfileSkills_IncludesSkillDetails()
    {
        var skill = new SkillEntity
        {
            Name = "Clean Code",
            Category = "Craftsmanship",
            LevelCount = 3,
            Description = "Writing readable code"
        };
        Db.Skills.Add(skill);
        var profile = new CompetenceCentreProfileEntity { Name = ".NET" };
        Db.CompetenceCentreProfiles.Add(profile);
        await Db.SaveChangesAsync();

        Db.CompetenceCentreProfileSkills.Add(new CompetenceCentreProfileSkillEntity
        {
            ProfileId = profile.Id,
            SkillId = skill.Id
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetProfileSkills(profile.Id);

        var ok = result.Result as OkObjectResult;
        var skills = ok!.Value as List<SkillSummaryDto>;
        Assert.That(skills![0].LevelCount, Is.EqualTo(3));
        Assert.That(skills[0].Description, Is.EqualTo("Writing readable code"));
    }
}
