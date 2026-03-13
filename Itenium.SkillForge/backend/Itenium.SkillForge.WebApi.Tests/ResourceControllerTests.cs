using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class ResourceControllerTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private ResourceController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _user.UserId.Returns("user-1");
        _user.IsBackOffice.Returns(true);
        _sut = new ResourceController(Db, _user);
    }

    private async Task<ResourceEntity> SeedResource(string title = "Test Resource", int? skillId = null)
    {
        var resource = new ResourceEntity
        {
            Title = title,
            Url = "https://example.com",
            Type = "article",
            SkillId = skillId,
            AddedByUserId = "user-1",
        };
        Db.Resources.Add(resource);
        await Db.SaveChangesAsync();
        return resource;
    }

    [Test]
    public async Task GetResources_ReturnsEmptyWhenNoResources()
    {
        var result = await _sut.GetResources();

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var resources = ok!.Value as IReadOnlyList<ResourceDto>;
        Assert.That(resources, Is.Empty);
    }

    [Test]
    public async Task GetResources_ReturnsAllResources()
    {
        await SeedResource("Clean Code Book");
        await SeedResource("DDD Video");

        var result = await _sut.GetResources();

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var resources = ok!.Value as IReadOnlyList<ResourceDto>;
        Assert.That(resources, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetResources_IncludesCompletionStatusForCurrentUser()
    {
        var resource = await SeedResource();
        Db.ResourceCompletions.Add(new ResourceCompletionEntity
        {
            ResourceId = resource.Id,
            UserId = "user-1",
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetResources();

        var ok = result.Result as OkObjectResult;
        var resources = ok!.Value as IReadOnlyList<ResourceDto>;
        Assert.That(resources![0].CompletedByCurrentUser, Is.True);
    }

    [Test]
    public async Task GetResources_IncludesRatingForCurrentUser()
    {
        var resource = await SeedResource();
        Db.ResourceRatings.Add(new ResourceRatingEntity
        {
            ResourceId = resource.Id,
            UserId = "user-1",
            IsPositive = true,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.GetResources();

        var ok = result.Result as OkObjectResult;
        var resources = ok!.Value as IReadOnlyList<ResourceDto>;
        Assert.That(resources![0].CurrentUserRating, Is.True);
    }

    [Test]
    public async Task GetResources_IncludesThumbsCounts()
    {
        var resource = await SeedResource();
        Db.ResourceRatings.AddRange(
            new ResourceRatingEntity { ResourceId = resource.Id, UserId = "user-2", IsPositive = true },
            new ResourceRatingEntity { ResourceId = resource.Id, UserId = "user-3", IsPositive = true },
            new ResourceRatingEntity { ResourceId = resource.Id, UserId = "user-4", IsPositive = false }
        );
        await Db.SaveChangesAsync();

        var result = await _sut.GetResources();

        var ok = result.Result as OkObjectResult;
        var resources = ok!.Value as IReadOnlyList<ResourceDto>;
        Assert.That(resources![0].ThumbsUp, Is.EqualTo(2));
        Assert.That(resources![0].ThumbsDown, Is.EqualTo(1));
    }

    [Test]
    public async Task CreateResource_ReturnsCreated()
    {
        var request = new CreateResourceRequest("My Article", "https://example.com", "article", null, null, null);

        var result = await _sut.CreateResource(request);

        var created = result.Result as CreatedAtActionResult;
        Assert.That(created, Is.Not.Null);
        var dto = created!.Value as ResourceDto;
        Assert.That(dto!.Title, Is.EqualTo("My Article"));
        Assert.That(dto.AddedByUserId, Is.EqualTo("user-1"));
    }

    [Test]
    public async Task CreateResource_WithInvalidSkillId_ReturnsBadRequest()
    {
        var request = new CreateResourceRequest("My Article", "https://example.com", "article", 99999, null, null);

        var result = await _sut.CreateResource(request);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task MarkComplete_ReturnsNoContent()
    {
        var resource = await SeedResource();

        var result = await _sut.MarkComplete(resource.Id);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        var completion = Db.ResourceCompletions.FirstOrDefault(c => c.ResourceId == resource.Id && c.UserId == "user-1");
        Assert.That(completion, Is.Not.Null);
    }

    [Test]
    public async Task MarkComplete_WhenAlreadyCompleted_IsIdempotent()
    {
        var resource = await SeedResource();
        Db.ResourceCompletions.Add(new ResourceCompletionEntity
        {
            ResourceId = resource.Id,
            UserId = "user-1",
        });
        await Db.SaveChangesAsync();

        var result = await _sut.MarkComplete(resource.Id);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        var count = Db.ResourceCompletions.Count(c => c.ResourceId == resource.Id && c.UserId == "user-1");
        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public async Task UnmarkComplete_ReturnsNoContent()
    {
        var resource = await SeedResource();
        Db.ResourceCompletions.Add(new ResourceCompletionEntity
        {
            ResourceId = resource.Id,
            UserId = "user-1",
        });
        await Db.SaveChangesAsync();

        var result = await _sut.UnmarkComplete(resource.Id);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        var completion = Db.ResourceCompletions.FirstOrDefault(c => c.ResourceId == resource.Id && c.UserId == "user-1");
        Assert.That(completion, Is.Null);
    }

    [Test]
    public async Task RateResource_ThumbsUp_ReturnsNoContent()
    {
        var resource = await SeedResource();

        var result = await _sut.RateResource(resource.Id, new RateResourceRequest(true));

        Assert.That(result, Is.TypeOf<NoContentResult>());
        var rating = Db.ResourceRatings.FirstOrDefault(r => r.ResourceId == resource.Id && r.UserId == "user-1");
        Assert.That(rating, Is.Not.Null);
        Assert.That(rating!.IsPositive, Is.True);
    }

    [Test]
    public async Task RateResource_ChangesExistingVote()
    {
        var resource = await SeedResource();
        Db.ResourceRatings.Add(new ResourceRatingEntity
        {
            ResourceId = resource.Id,
            UserId = "user-1",
            IsPositive = true,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.RateResource(resource.Id, new RateResourceRequest(false));

        Assert.That(result, Is.TypeOf<NoContentResult>());
        var rating = Db.ResourceRatings.FirstOrDefault(r => r.ResourceId == resource.Id && r.UserId == "user-1");
        Assert.That(rating!.IsPositive, Is.False);
    }

    [Test]
    public async Task RemoveRating_ReturnsNoContent()
    {
        var resource = await SeedResource();
        Db.ResourceRatings.Add(new ResourceRatingEntity
        {
            ResourceId = resource.Id,
            UserId = "user-1",
            IsPositive = true,
        });
        await Db.SaveChangesAsync();

        var result = await _sut.RemoveRating(resource.Id);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        var rating = Db.ResourceRatings.FirstOrDefault(r => r.ResourceId == resource.Id && r.UserId == "user-1");
        Assert.That(rating, Is.Null);
    }
}
