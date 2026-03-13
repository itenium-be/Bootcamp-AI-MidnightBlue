using Itenium.SkillForge.Entities;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class ResourceControllerTests : DatabaseTestBase
{
    private ResourceController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _sut = new ResourceController(Db);
    }

    [Test]
    public async Task GetResources_ReturnsAllResources()
    {
        Db.Resources.AddRange(
            new ResourceEntity { Title = "Intro to C#", Url = "https://example.com/1", Type = ResourceType.Article },
            new ResourceEntity { Title = "Advanced .NET", Url = "https://example.com/2", Type = ResourceType.Video });
        await Db.SaveChangesAsync();

        var result = await _sut.GetResources(null, null);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var resources = okResult!.Value as List<ResourceEntity>;
        Assert.That(resources, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetResources_WhenNoResources_ReturnsEmptyList()
    {
        var result = await _sut.GetResources(null, null);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var resources = okResult!.Value as List<ResourceEntity>;
        Assert.That(resources, Is.Empty);
    }

    [Test]
    public async Task GetResources_FilterBySkillId_ReturnsMatchingOnly()
    {
        Db.Resources.AddRange(
            new ResourceEntity { Title = "C# Basics", Url = "https://example.com/1", Type = ResourceType.Article, SkillId = 1 },
            new ResourceEntity { Title = "Java Basics", Url = "https://example.com/2", Type = ResourceType.Video, SkillId = 2 },
            new ResourceEntity { Title = "No Skill", Url = "https://example.com/3", Type = ResourceType.Book });
        await Db.SaveChangesAsync();

        var result = await _sut.GetResources(skillId: 1, type: null);

        var okResult = result.Result as OkObjectResult;
        var resources = okResult!.Value as List<ResourceEntity>;
        Assert.That(resources, Has.Count.EqualTo(1));
        Assert.That(resources![0].Title, Is.EqualTo("C# Basics"));
    }

    [Test]
    public async Task GetResources_FilterByType_ReturnsMatchingOnly()
    {
        Db.Resources.AddRange(
            new ResourceEntity { Title = "Article 1", Url = "https://example.com/1", Type = ResourceType.Article },
            new ResourceEntity { Title = "Video 1", Url = "https://example.com/2", Type = ResourceType.Video },
            new ResourceEntity { Title = "Article 2", Url = "https://example.com/3", Type = ResourceType.Article });
        await Db.SaveChangesAsync();

        var result = await _sut.GetResources(skillId: null, type: ResourceType.Article);

        var okResult = result.Result as OkObjectResult;
        var resources = okResult!.Value as List<ResourceEntity>;
        Assert.That(resources, Has.Count.EqualTo(2));
        Assert.That(resources!.All(r => r.Type == ResourceType.Article), Is.True);
    }

    [Test]
    public async Task GetResources_FilterBySkillIdAndType_ReturnsMatchingOnly()
    {
        Db.Resources.AddRange(
            new ResourceEntity { Title = "Match", Url = "https://example.com/1", Type = ResourceType.Article, SkillId = 1 },
            new ResourceEntity { Title = "Wrong type", Url = "https://example.com/2", Type = ResourceType.Video, SkillId = 1 },
            new ResourceEntity { Title = "Wrong skill", Url = "https://example.com/3", Type = ResourceType.Article, SkillId = 2 });
        await Db.SaveChangesAsync();

        var result = await _sut.GetResources(skillId: 1, type: ResourceType.Article);

        var okResult = result.Result as OkObjectResult;
        var resources = okResult!.Value as List<ResourceEntity>;
        Assert.That(resources, Has.Count.EqualTo(1));
        Assert.That(resources![0].Title, Is.EqualTo("Match"));
    }
}
