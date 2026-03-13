using Itenium.SkillForge.Entities;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class SkillControllerTests : DatabaseTestBase
{
    private SkillController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _sut = new SkillController(Db);
    }

    [Test]
    public async Task GetSkills_ReturnsAllSkills()
    {
        Db.Skills.AddRange(
            new SkillEntity { Name = "Clean Code", Category = "Craftsmanship" },
            new SkillEntity { Name = "C# Language", Category = ".NET" });
        await Db.SaveChangesAsync();

        var result = await _sut.GetSkills();

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var skills = ok!.Value as List<SkillSummaryDto>;
        Assert.That(skills, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetSkills_WhenEmpty_ReturnsEmptyList()
    {
        var result = await _sut.GetSkills();

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var skills = ok!.Value as List<SkillSummaryDto>;
        Assert.That(skills, Is.Empty);
    }

    [Test]
    public async Task GetSkill_WhenExists_ReturnsSkillWithLevelDescriptors()
    {
        var skill = new SkillEntity
        {
            Name = "Clean Code",
            Category = "Craftsmanship",
            Description = "Writing readable, maintainable code",
            LevelCount = 3,
            LevelDescriptors =
            [
                new SkillLevelDescriptorEntity { Level = 1, Description = "Aware of naming and formatting conventions" },
                new SkillLevelDescriptorEntity { Level = 2, Description = "Applies clean code at function and class level" },
                new SkillLevelDescriptorEntity { Level = 3, Description = "Applies clean code at architectural level" },
            ]
        };
        Db.Skills.Add(skill);
        await Db.SaveChangesAsync();

        var result = await _sut.GetSkill(skill.Id);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as SkillDetailDto;
        Assert.That(dto!.Name, Is.EqualTo("Clean Code"));
        Assert.That(dto.LevelCount, Is.EqualTo(3));
        Assert.That(dto.LevelDescriptors, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task GetSkill_WhenExists_ReturnsPrerequisiteLinks()
    {
        var prereq = new SkillEntity { Name = "Clean Code", Category = "Craftsmanship", LevelCount = 3 };
        var skill = new SkillEntity
        {
            Name = "Domain-Driven Design",
            Category = "Architecture",
            LevelCount = 3,
        };
        Db.Skills.AddRange(prereq, skill);
        await Db.SaveChangesAsync();

        Db.SkillPrerequisites.Add(new SkillPrerequisiteEntity
        {
            SkillId = skill.Id,
            RequiredSkillId = prereq.Id,
            RequiredLevel = 3
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetSkill(skill.Id);

        var ok = result.Result as OkObjectResult;
        var dto = ok!.Value as SkillDetailDto;
        Assert.That(dto!.Prerequisites, Has.Count.EqualTo(1));
        Assert.That(dto.Prerequisites[0].RequiredSkillId, Is.EqualTo(prereq.Id));
        Assert.That(dto.Prerequisites[0].RequiredLevel, Is.EqualTo(3));
    }

    [Test]
    public async Task GetSkill_WhenNotExists_ReturnsNotFound()
    {
        var result = await _sut.GetSkill(999);
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetSkillsByCategory_GroupsSkillsByCategory()
    {
        Db.Skills.AddRange(
            new SkillEntity { Name = "Clean Code", Category = "Craftsmanship" },
            new SkillEntity { Name = "SOLID Principles", Category = "Craftsmanship" },
            new SkillEntity { Name = "C# Language", Category = ".NET" });
        await Db.SaveChangesAsync();

        var result = await _sut.GetSkillsByCategory();

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var categories = ok!.Value as List<SkillCategoryDto>;
        Assert.That(categories, Has.Count.EqualTo(2));
        var craftsmanship = categories!.First(c => c.Category == "Craftsmanship");
        Assert.That(craftsmanship.Skills, Has.Count.EqualTo(2));
    }
}
