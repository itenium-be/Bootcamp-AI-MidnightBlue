using System.Security.Claims;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class ResourceRatingControllerTests : DatabaseTestBase
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
    public async Task RateResource_Upvote_IncrementsUpvoteCount()
    {
        var resource = await AddResource();

        var result = await _sut.RateResource(resource.Id, new RateResourceRequest(true));

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        await Db.Entry(resource).ReloadAsync();
        Assert.That(resource.Upvotes, Is.EqualTo(1));
        Assert.That(resource.Downvotes, Is.EqualTo(0));
    }

    [Test]
    public async Task RateResource_Downvote_IncrementsDownvoteCount()
    {
        var resource = await AddResource();

        await _sut.RateResource(resource.Id, new RateResourceRequest(false));

        await Db.Entry(resource).ReloadAsync();
        Assert.That(resource.Upvotes, Is.EqualTo(0));
        Assert.That(resource.Downvotes, Is.EqualTo(1));
    }

    [Test]
    public async Task RateResource_WhenResourceNotFound_ReturnsNotFound()
    {
        var result = await _sut.RateResource(999, new RateResourceRequest(true));
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task RateResource_WhenSameVoteSubmittedAgain_IsIdempotent()
    {
        var resource = await AddResource();
        await _sut.RateResource(resource.Id, new RateResourceRequest(true));

        await _sut.RateResource(resource.Id, new RateResourceRequest(true));

        await Db.Entry(resource).ReloadAsync();
        Assert.That(resource.Upvotes, Is.EqualTo(1));
        var ratings = Db.ResourceRatings
            .Where(r => r.ConsultantId == "testuser" && r.ResourceId == resource.Id)
            .ToList();
        Assert.That(ratings, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task RateResource_WhenChangingVote_UpdatesBothCounters()
    {
        var resource = await AddResource();
        await _sut.RateResource(resource.Id, new RateResourceRequest(true));

        await _sut.RateResource(resource.Id, new RateResourceRequest(false));

        await Db.Entry(resource).ReloadAsync();
        Assert.That(resource.Upvotes, Is.EqualTo(0));
        Assert.That(resource.Downvotes, Is.EqualTo(1));
    }

    [Test]
    public async Task GetMyRatings_ReturnsCurrentUserRatings()
    {
        var r1 = await AddResource("Resource 1");
        var r2 = await AddResource("Resource 2");
        await _sut.RateResource(r1.Id, new RateResourceRequest(true));
        await _sut.RateResource(r2.Id, new RateResourceRequest(false));

        var result = await _sut.GetMyRatings();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var ratings = okResult!.Value as List<MyRatingDto>;
        Assert.That(ratings, Has.Count.EqualTo(2));
        Assert.That(ratings!.Single(r => r.ResourceId == r1.Id).IsUpvote, Is.True);
        Assert.That(ratings!.Single(r => r.ResourceId == r2.Id).IsUpvote, Is.False);
    }

    [Test]
    public async Task GetMyRatings_OnlyReturnsCurrentUserRatings()
    {
        var resource = await AddResource();
        Db.ResourceRatings.Add(new ResourceRatingEntity
        {
            ConsultantId = "otheruser",
            ResourceId = resource.Id,
            IsUpvote = true,
        });
        await Db.SaveChangesAsync();
        await _sut.RateResource(resource.Id, new RateResourceRequest(false));

        var result = await _sut.GetMyRatings();

        var okResult = result.Result as OkObjectResult;
        var ratings = okResult!.Value as List<MyRatingDto>;
        Assert.That(ratings, Has.Count.EqualTo(1));
        Assert.That(ratings![0].IsUpvote, Is.False);
    }

    [Test]
    public async Task RemoveRating_DeletesRatingAndDecrementsCounter()
    {
        var resource = await AddResource();
        await _sut.RateResource(resource.Id, new RateResourceRequest(true));

        var result = await _sut.RemoveRating(resource.Id);

        Assert.That(result, Is.TypeOf<NoContentResult>());
        Assert.That(Db.ResourceRatings.Any(r => r.ConsultantId == "testuser" && r.ResourceId == resource.Id), Is.False);
        await Db.Entry(resource).ReloadAsync();
        Assert.That(resource.Upvotes, Is.EqualTo(0));
    }

    [Test]
    public async Task RemoveRating_WhenNotRated_ReturnsNotFound()
    {
        var resource = await AddResource();

        var result = await _sut.RemoveRating(resource.Id);

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }
}
