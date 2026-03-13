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
            new SkillEntity { Name = "C# Basics" },
            new SkillEntity { Name = "Advanced .NET" });
        await Db.SaveChangesAsync();

        var result = await _sut.GetSkills();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var skills = okResult!.Value as List<SkillEntity>;
        Assert.That(skills, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetSkills_WhenNoSkills_ReturnsEmptyList()
    {
        var result = await _sut.GetSkills();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var skills = okResult!.Value as List<SkillEntity>;
        Assert.That(skills, Is.Empty);
    }

    [Test]
    public async Task GetSkill_WhenExists_ReturnsSkill()
    {
        var skill = new SkillEntity { Name = "C# Basics", Description = "Learn C#" };
        Db.Skills.Add(skill);
        await Db.SaveChangesAsync();

        var result = await _sut.GetSkill(skill.Id);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var returnedSkill = okResult!.Value as SkillEntity;
        Assert.That(returnedSkill!.Name, Is.EqualTo("C# Basics"));
        Assert.That(returnedSkill.Description, Is.EqualTo("Learn C#"));
    }

    [Test]
    public async Task GetSkill_WhenNotExists_ReturnsNotFound()
    {
        var result = await _sut.GetSkill(999);
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task CreateSkill_AddsSkillAndReturnsCreated()
    {
        var request = new CreateSkillRequest("New Skill", "Description", "Programming", 1, null);

        var result = await _sut.CreateSkill(request);

        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        var skill = createdResult!.Value as SkillEntity;
        Assert.That(skill!.Name, Is.EqualTo("New Skill"));
        Assert.That(skill.Description, Is.EqualTo("Description"));
        Assert.That(skill.Category, Is.EqualTo("Programming"));
        Assert.That(skill.LevelCount, Is.EqualTo(1));

        var savedSkill = await Db.Skills.FindAsync(skill.Id);
        Assert.That(savedSkill, Is.Not.Null);
        Assert.That(savedSkill!.Name, Is.EqualTo("New Skill"));
    }

    [Test]
    public async Task CreateSkill_WithLevelCountAndDescriptors_PersistsCorrectly()
    {
        IList<string> descriptors = ["Aware", "Practitioner", "Expert"];
        var request = new CreateSkillRequest("C# Advanced", null, "Programming", 3, descriptors);

        var result = await _sut.CreateSkill(request);

        var createdResult = result.Result as CreatedAtActionResult;
        var skill = createdResult!.Value as SkillEntity;
        Assert.That(skill!.LevelCount, Is.EqualTo(3));
        Assert.That(skill.LevelDescriptors, Is.EqualTo(descriptors));

        var savedSkill = await Db.Skills.FindAsync(skill.Id);
        Assert.That(savedSkill!.LevelCount, Is.EqualTo(3));
        Assert.That(savedSkill.LevelDescriptors, Is.EqualTo(descriptors));
    }

    [Test]
    public async Task CreateSkill_WithLevelCount1_IsCheckbox()
    {
        var request = new CreateSkillRequest("Git Basics", null, "DevOps", 1, null);

        var result = await _sut.CreateSkill(request);

        var createdResult = result.Result as CreatedAtActionResult;
        var skill = createdResult!.Value as SkillEntity;
        Assert.That(skill!.LevelCount, Is.EqualTo(1));
        Assert.That(skill.LevelDescriptors, Is.Empty);
    }

    [Test]
    public async Task UpdateSkill_WhenExists_UpdatesAndReturnsOk()
    {
        var skill = new SkillEntity { Name = "Old Name", Description = "Old Desc" };
        Db.Skills.Add(skill);
        await Db.SaveChangesAsync();
        IList<string> descriptors = ["Basic", "Advanced"];
        var request = new UpdateSkillRequest("New Name", "New Desc", "New Category", 2, descriptors);

        var result = await _sut.UpdateSkill(skill.Id, request);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var updatedSkill = okResult!.Value as SkillEntity;
        Assert.That(updatedSkill!.Name, Is.EqualTo("New Name"));
        Assert.That(updatedSkill.Description, Is.EqualTo("New Desc"));
        Assert.That(updatedSkill.Category, Is.EqualTo("New Category"));
        Assert.That(updatedSkill.LevelCount, Is.EqualTo(2));
        Assert.That(updatedSkill.LevelDescriptors, Is.EqualTo(descriptors));
    }

    [Test]
    public async Task UpdateSkill_WhenNotExists_ReturnsNotFound()
    {
        var request = new UpdateSkillRequest("Name", "Desc", null, 1, null);
        var result = await _sut.UpdateSkill(999, request);
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task DeleteSkill_WhenExists_RemovesAndReturnsNoContent()
    {
        var skill = new SkillEntity { Name = "To Delete" };
        Db.Skills.Add(skill);
        await Db.SaveChangesAsync();

        var result = await _sut.DeleteSkill(skill.Id);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        var deletedSkill = await Db.Skills.FindAsync(skill.Id);
        Assert.That(deletedSkill, Is.Null);
    }

    [Test]
    public async Task DeleteSkill_WhenNotExists_ReturnsNotFound()
    {
        var result = await _sut.DeleteSkill(999);
        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }
}
