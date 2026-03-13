using System.Globalization;
using Itenium.Forge.Security;
using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class UserControllerTests : IdentityTestBase
{
    private UserController _sut = null!;
    private ISkillForgeUser _currentUser = null!;

    [SetUp]
    public void Setup()
    {
        _currentUser = Substitute.For<ISkillForgeUser>();
        _currentUser.UserId.Returns("test-admin-id");
        _sut = new UserController(UserManager, Db, _currentUser);
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

    // SF-31: Archive User

    [Test]
    public async Task ArchiveUser_WhenExists_ReturnsNoContent()
    {
        var created = await _sut.CreateUser(new CreateUserRequest("Alice", "Smith", "alice@test.local", "learner", [], "Password123!"));
        var user = (created.Result as CreatedAtActionResult)!.Value as UserResponse;

        var result = await _sut.ArchiveUser(user!.Id);

        Assert.That(result, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task ArchiveUser_WhenNotExists_ReturnsNotFound()
    {
        var result = await _sut.ArchiveUser("nonexistent-id");

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task ArchiveUser_WhenAlreadyArchived_ReturnsBadRequest()
    {
        var created = await _sut.CreateUser(new CreateUserRequest("Alice", "Smith", "alice@test.local", "learner", [], "Password123!"));
        var user = (created.Result as CreatedAtActionResult)!.Value as UserResponse;
        await _sut.ArchiveUser(user!.Id);

        var result = await _sut.ArchiveUser(user.Id);

        Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task GetUsers_ExcludesArchivedUsers()
    {
        var created = await _sut.CreateUser(new CreateUserRequest("Alice", "Smith", "alice@test.local", "learner", [], "Password123!"));
        var user = (created.Result as CreatedAtActionResult)!.Value as UserResponse;
        await _sut.ArchiveUser(user!.Id);

        var result = await _sut.GetUsers();

        var okResult = result.Result as OkObjectResult;
        var users = okResult!.Value as List<UserResponse>;
        Assert.That(users, Is.Empty);
    }

    // SF-32: Archived Users & Restore

    [Test]
    public async Task GetArchivedUsers_ReturnsOnlyArchivedUsers()
    {
        var created = await _sut.CreateUser(new CreateUserRequest("Alice", "Smith", "alice@test.local", "learner", [], "Password123!"));
        var user = (created.Result as CreatedAtActionResult)!.Value as UserResponse;
        await _sut.CreateUser(new CreateUserRequest("Bob", "Jones", "bob@test.local", "learner", [], "Password123!"));
        await _sut.ArchiveUser(user!.Id);

        var result = await _sut.GetArchivedUsers();

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var archivedUsers = okResult!.Value as List<ArchivedUserResponse>;
        Assert.That(archivedUsers, Has.Count.EqualTo(1));
        Assert.That(archivedUsers![0].Email, Is.EqualTo("alice@test.local"));
        Assert.That(archivedUsers[0].ArchivedBy, Is.EqualTo("test-admin-id"));
    }

    [Test]
    public async Task RestoreUser_WhenArchived_ReturnsUser()
    {
        var created = await _sut.CreateUser(new CreateUserRequest("Alice", "Smith", "alice@test.local", "learner", [], "Password123!"));
        var user = (created.Result as CreatedAtActionResult)!.Value as UserResponse;
        await _sut.ArchiveUser(user!.Id);

        var result = await _sut.RestoreUser(user.Id);

        var okResult = result.Result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var restored = okResult!.Value as UserResponse;
        Assert.That(restored!.Email, Is.EqualTo("alice@test.local"));
    }

    [Test]
    public async Task RestoreUser_WhenNotExists_ReturnsNotFound()
    {
        var result = await _sut.RestoreUser("nonexistent-id");

        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task RestoreUser_WhenNotArchived_ReturnsBadRequest()
    {
        var created = await _sut.CreateUser(new CreateUserRequest("Alice", "Smith", "alice@test.local", "learner", [], "Password123!"));
        var user = (created.Result as CreatedAtActionResult)!.Value as UserResponse;

        var result = await _sut.RestoreUser(user!.Id);

        Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task RestoreUser_RemovedFromArchivedList()
    {
        var created = await _sut.CreateUser(new CreateUserRequest("Alice", "Smith", "alice@test.local", "learner", [], "Password123!"));
        var user = (created.Result as CreatedAtActionResult)!.Value as UserResponse;
        await _sut.ArchiveUser(user!.Id);
        await _sut.RestoreUser(user.Id);

        var archivedResult = await _sut.GetArchivedUsers();
        var archivedUsers = (archivedResult.Result as OkObjectResult)!.Value as List<ArchivedUserResponse>;
        Assert.That(archivedUsers, Is.Empty);

        var activeResult = await _sut.GetUsers();
        var activeUsers = (activeResult.Result as OkObjectResult)!.Value as List<UserResponse>;
        Assert.That(activeUsers, Has.Count.EqualTo(1));
    }

    // SF-33: Uncoached Consultants

    [Test]
    public async Task GetUncoachedUsers_ReturnsLearnersWithNoTeam()
    {
        await _sut.CreateUser(new CreateUserRequest("Alice", "Smith", "alice@test.local", "learner", [], "Password123!"));

        var result = await _sut.GetUncoachedUsers();

        var okResult = result.Result as OkObjectResult;
        var users = okResult!.Value as List<UserResponse>;
        Assert.That(users, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetUncoachedUsers_ReturnsLearnersWithNoActiveManager()
    {
        var team = new TeamEntity { Name = "Java" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        await _sut.CreateUser(new CreateUserRequest("Alice", "Smith", "alice@test.local", "learner", [team.Id], "Password123!"));

        var result = await _sut.GetUncoachedUsers();

        var okResult = result.Result as OkObjectResult;
        var users = okResult!.Value as List<UserResponse>;
        Assert.That(users, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GetUncoachedUsers_ExcludesLearnersWithActiveManager()
    {
        var team = new TeamEntity { Name = "Java" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        await _sut.CreateUser(new CreateUserRequest("Alice", "Smith", "alice@test.local", "learner", [team.Id], "Password123!"));
        await _sut.CreateUser(new CreateUserRequest("Bob", "Jones", "bob@test.local", "manager", [team.Id], "Password123!"));

        var result = await _sut.GetUncoachedUsers();

        var okResult = result.Result as OkObjectResult;
        var users = okResult!.Value as List<UserResponse>;
        Assert.That(users, Is.Empty);
    }

    [Test]
    public async Task GetUncoachedUsers_ReturnsLearnersWhoseManagerIsArchived()
    {
        var team = new TeamEntity { Name = "Java" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        await _sut.CreateUser(new CreateUserRequest("Alice", "Smith", "alice@test.local", "learner", [team.Id], "Password123!"));
        var managerResult = await _sut.CreateUser(new CreateUserRequest("Bob", "Jones", "bob@test.local", "manager", [team.Id], "Password123!"));
        var manager = (managerResult.Result as CreatedAtActionResult)!.Value as UserResponse;
        await _sut.ArchiveUser(manager!.Id);

        var result = await _sut.GetUncoachedUsers();

        var okResult = result.Result as OkObjectResult;
        var users = okResult!.Value as List<UserResponse>;
        Assert.That(users, Has.Count.EqualTo(1));
        Assert.That(users![0].Email, Is.EqualTo("alice@test.local"));
    }
}
