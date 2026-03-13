using System.Security.Claims;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class GoalControllerTests : DatabaseTestBase
{
    private GoalController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _sut = new GoalController(Db);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.Name, "testuser")], "test")),
            },
        };
    }

    private async Task<GoalEntity> AddGoal(string consultantId = "testuser", string title = "Clean Code niveau 3")
    {
        var goal = new GoalEntity { ConsultantId = consultantId, Title = title, CreatedBy = "coach" };
        Db.Goals.Add(goal);
        await Db.SaveChangesAsync();
        return goal;
    }

    [Test]
    public async Task GetMyGoals_ReturnsCurrentUserGoals()
    {
        await AddGoal("testuser", "Clean Code niveau 3");
        await AddGoal("testuser", "Entity Framework niveau 2");

        var result = await _sut.GetMyGoals();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var goals = okResult!.Value as List<GoalDto>;
        Assert.That(goals, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetMyGoals_DoesNotReturnOtherUsersGoals()
    {
        await AddGoal("testuser");
        await AddGoal("otheruser");

        var result = await _sut.GetMyGoals();

        var okResult = result.Result as OkObjectResult;
        var goals = okResult!.Value as List<GoalDto>;
        Assert.That(goals, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetMyGoals_IncludesFlagRaisedAt_WhenFlagExists()
    {
        var goal = await AddGoal();
        Db.ReadinessFlags.Add(new ReadinessFlagEntity { GoalId = goal.Id, ConsultantId = "testuser" });
        await Db.SaveChangesAsync();

        var result = await _sut.GetMyGoals();

        var okResult = result.Result as OkObjectResult;
        var goals = okResult!.Value as List<GoalDto>;
        Assert.That(goals![0].FlagRaisedAt, Is.Not.Null);
    }

    [Test]
    public async Task RaiseReadinessFlag_CreatesFlag()
    {
        var goal = await AddGoal();

        var result = await _sut.RaiseReadinessFlag(goal.Id);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var flag = okResult!.Value as ReadinessFlagEntity;
        Assert.That(flag!.GoalId, Is.EqualTo(goal.Id));
        Assert.That(flag.ConsultantId, Is.EqualTo("testuser"));
        Assert.That(flag.RaisedAt, Is.Not.EqualTo(default(DateTime)));
    }

    [Test]
    public async Task RaiseReadinessFlag_WhenGoalNotFound_ReturnsNotFound()
    {
        var result = await _sut.RaiseReadinessFlag(999);
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task RaiseReadinessFlag_WhenGoalBelongsToDifferentUser_ReturnsNotFound()
    {
        var goal = await AddGoal("otheruser");

        var result = await _sut.RaiseReadinessFlag(goal.Id);

        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task RaiseReadinessFlag_WhenAlreadyFlagged_IsIdempotent()
    {
        var goal = await AddGoal();
        await _sut.RaiseReadinessFlag(goal.Id);

        var result = await _sut.RaiseReadinessFlag(goal.Id);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(Db.ReadinessFlags.Count(f => f.GoalId == goal.Id), Is.EqualTo(1));
    }

    [Test]
    public async Task GetTeamFlags_ReturnsAllFlags()
    {
        var g1 = await AddGoal("user1", "Goal A");
        var g2 = await AddGoal("user2", "Goal B");
        Db.ReadinessFlags.AddRange(
            new ReadinessFlagEntity { GoalId = g1.Id, ConsultantId = "user1" },
            new ReadinessFlagEntity { GoalId = g2.Id, ConsultantId = "user2" });
        await Db.SaveChangesAsync();

        var result = await _sut.GetTeamFlags();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var flags = okResult!.Value as List<TeamFlagDto>;
        Assert.That(flags, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetTeamFlags_IncludesGoalTitleAndRaisedAt()
    {
        var goal = await AddGoal("user1", "Clean Code niveau 3");
        Db.ReadinessFlags.Add(new ReadinessFlagEntity { GoalId = goal.Id, ConsultantId = "user1" });
        await Db.SaveChangesAsync();

        var result = await _sut.GetTeamFlags();

        var okResult = result.Result as OkObjectResult;
        var flags = okResult!.Value as List<TeamFlagDto>;
        Assert.That(flags![0].GoalTitle, Is.EqualTo("Clean Code niveau 3"));
        Assert.That(flags![0].ConsultantId, Is.EqualTo("user1"));
        Assert.That(flags![0].RaisedAt, Is.Not.EqualTo(default(DateTime)));
    }
}
