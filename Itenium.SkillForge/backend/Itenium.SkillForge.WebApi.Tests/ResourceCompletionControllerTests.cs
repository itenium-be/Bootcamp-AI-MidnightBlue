using System.Security.Claims;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class ResourceCompletionControllerTests : DatabaseTestBase
{
    private ResourceController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _sut = new ResourceController(Db);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.Name, "testuser")], "test")),
            },
        };
    }

    private async Task<ResourceEntity> AddResource(string title = "Test Resource")
    {
        var resource = new ResourceEntity { Title = title, Url = "https://example.com", Type = ResourceType.Article };
        Db.Resources.Add(resource);
        await Db.SaveChangesAsync();
        return resource;
    }

    [Test]
    public async Task MarkCompleted_CreatesCompletionRecord()
    {
        var resource = await AddResource();

        var result = await _sut.MarkCompleted(resource.Id);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var completion = okResult!.Value as ResourceCompletionEntity;
        Assert.That(completion!.ConsultantId, Is.EqualTo("testuser"));
        Assert.That(completion.ResourceId, Is.EqualTo(resource.Id));
        Assert.That(completion.CompletedAt, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public async Task MarkCompleted_WhenResourceNotFound_ReturnsNotFound()
    {
        var result = await _sut.MarkCompleted(999);
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task MarkCompleted_WhenAlreadyCompleted_IsIdempotentAndReturnsExisting()
    {
        var resource = await AddResource();
        await _sut.MarkCompleted(resource.Id);

        var result = await _sut.MarkCompleted(resource.Id);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var completions = Db.ResourceCompletions
            .Where(c => c.ConsultantId == "testuser" && c.ResourceId == resource.Id)
            .ToList();
        Assert.That(completions, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetMyCompletions_ReturnsCompletedResourceIds()
    {
        var r1 = await AddResource("Resource 1");
        var r2 = await AddResource("Resource 2");
        await AddResource("Resource 3");
        await _sut.MarkCompleted(r1.Id);
        await _sut.MarkCompleted(r2.Id);

        var result = await _sut.GetMyCompletions();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var ids = okResult!.Value as List<int>;
        Assert.That(ids, Has.Count.EqualTo(2));
        Assert.That(ids, Contains.Item(r1.Id));
        Assert.That(ids, Contains.Item(r2.Id));
    }

    [Test]
    public async Task GetMyCompletions_OnlyReturnsCurrentUserCompletions()
    {
        var resource = await AddResource();
        Db.ResourceCompletions.Add(new ResourceCompletionEntity
        {
            ConsultantId = "otheruser",
            ResourceId = resource.Id,
        });
        await Db.SaveChangesAsync();
        await _sut.MarkCompleted(resource.Id);

        var result = await _sut.GetMyCompletions();

        var okResult = result.Result as OkObjectResult;
        var ids = okResult!.Value as List<int>;
        Assert.That(ids, Has.Count.EqualTo(1));
    }

}
