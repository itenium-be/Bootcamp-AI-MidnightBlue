using System.Globalization;
using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class UserControllerTests : IdentityTestBase
{
    private UserController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _sut = new UserController(UserManager, Db);
    }

    [Test]
    public async Task GetUsers_WhenNoUsers_ReturnsEmptyList()
    {
        var result = await _sut.GetUsers();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var users = okResult!.Value as List<UserResponse>;
        Assert.That(users, Is.Empty);
    }

    [Test]
    public async Task GetUsers_WhenUsersExist_ReturnsAllUsers()
    {
        await UserManager.CreateAsync(new ForgeUser
        {
            UserName = "alice@test.local",
            Email = "alice@test.local",
            FirstName = "Alice",
            LastName = "Smith",
            EmailConfirmed = true
        }, "Password123!");
        await UserManager.CreateAsync(new ForgeUser
        {
            UserName = "bob@test.local",
            Email = "bob@test.local",
            FirstName = "Bob",
            LastName = "Jones",
            EmailConfirmed = true
        }, "Password123!");

        var result = await _sut.GetUsers();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var users = okResult!.Value as List<UserResponse>;
        Assert.That(users, Has.Count.EqualTo(2));
        Assert.That(users!.Select(u => u.Email), Contains.Item("alice@test.local"));
        Assert.That(users.Select(u => u.Email), Contains.Item("bob@test.local"));
    }

    [Test]
    public async Task GetUsers_ReturnsRoleAndTeamsPerUser()
    {
        var team = new TeamEntity { Name = "Java" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        var request = new CreateUserRequest("Alice", "Smith", "alice@test.local", "manager", [team.Id], "Password123!");
        await _sut.CreateUser(request);

        var result = await _sut.GetUsers();

        var okResult = result.Result as OkObjectResult;
        var users = okResult!.Value as List<UserResponse>;
        var alice = users!.Single();
        Assert.That(alice.Role, Is.EqualTo("manager"));
        Assert.That(alice.TeamIds, Contains.Item(team.Id));
    }

    [Test]
    public async Task CreateUser_WithValidData_ReturnsCreated()
    {
        var request = new CreateUserRequest("Alice", "Smith", "alice@test.local", "learner", [], "Password123!");

        var result = await _sut.CreateUser(request);

        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult, Is.Not.Null);
        var user = createdResult!.Value as UserResponse;
        Assert.That(user!.FirstName, Is.EqualTo("Alice"));
        Assert.That(user.LastName, Is.EqualTo("Smith"));
        Assert.That(user.Email, Is.EqualTo("alice@test.local"));
    }

    [Test]
    public async Task CreateUser_WithRole_AssignsRole()
    {
        var request = new CreateUserRequest("Alice", "Smith", "alice@test.local", "manager", [], "Password123!");

        await _sut.CreateUser(request);

        var createdUser = await UserManager.FindByEmailAsync("alice@test.local");
        Assert.That(createdUser, Is.Not.Null);
        var roles = await UserManager.GetRolesAsync(createdUser!);
        Assert.That(roles, Contains.Item("manager"));
    }

    [Test]
    public async Task CreateUser_WithTeams_AssignsTeamClaims()
    {
        var java = new TeamEntity { Name = "Java" };
        var dotnet = new TeamEntity { Name = ".NET" };
        Db.Teams.AddRange(java, dotnet);
        await Db.SaveChangesAsync();

        var request = new CreateUserRequest("Alice", "Smith", "alice@test.local", "manager", [java.Id, dotnet.Id], "Password123!");

        await _sut.CreateUser(request);

        var createdUser = await UserManager.FindByEmailAsync("alice@test.local");
        var claims = await UserManager.GetClaimsAsync(createdUser!);
        var teamIds = claims.Where(c => c.Type == "team").Select(c => int.Parse(c.Value, CultureInfo.InvariantCulture)).ToArray();
        Assert.That(teamIds, Contains.Item(java.Id));
        Assert.That(teamIds, Contains.Item(dotnet.Id));
    }

    [Test]
    public async Task CreateUser_WithDuplicateEmail_ReturnsBadRequest()
    {
        var request = new CreateUserRequest("Alice", "Smith", "alice@test.local", "learner", [], "Password123!");
        await _sut.CreateUser(request);

        var result = await _sut.CreateUser(request);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetUser_WhenExists_ReturnsUser()
    {
        var request = new CreateUserRequest("Alice", "Smith", "alice@test.local", "learner", [], "Password123!");
        var created = await _sut.CreateUser(request);
        var createdUser = (created.Result as CreatedAtActionResult)!.Value as UserResponse;

        var result = await _sut.GetUser(createdUser!.Id);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var user = okResult!.Value as UserResponse;
        Assert.That(user!.Email, Is.EqualTo("alice@test.local"));
    }

    [Test]
    public async Task GetUser_WhenNotExists_ReturnsNotFound()
    {
        var result = await _sut.GetUser("nonexistent-id");
        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }
}
