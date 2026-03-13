using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class ConsultantControllerTests : DatabaseTestBase
{
    private ISkillForgeUser _user = null!;
    private ConsultantController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _user = Substitute.For<ISkillForgeUser>();
        _sut = new ConsultantController(Db, _user);
    }

    private async Task<(TeamEntity team, string userId)> SeedConsultant(
        string firstName,
        string lastName,
        int teamId,
        DateTime? lastActivityAt = null)
    {
        var team = await Db.Teams.FindAsync(teamId)
            ?? (TeamEntity)Db.Teams.Add(new TeamEntity { Id = teamId, Name = $"Team{teamId}" }).Entity;
        await Db.SaveChangesAsync();

        var userId = Guid.NewGuid().ToString();
        var nameLower = $"{firstName}{lastName}".ToLowerInvariant();
        var emailLocal = firstName.ToLowerInvariant();
        Db.Users.Add(new ForgeUser
        {
            Id = userId,
            UserName = nameLower,
            Email = $"{emailLocal}@test.local",
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
        });
        Db.ConsultantProfiles.Add(new ConsultantProfileEntity
        {
            UserId = userId,
            TeamId = teamId,
            LastActivityAt = lastActivityAt,
        });
        await Db.SaveChangesAsync();
        return (team, userId);
    }

    [Test]
    public async Task GetConsultants_WhenManager_ReturnsOnlyOwnTeamConsultants()
    {
        var team1 = new TeamEntity { Name = "Java" };
        var team2 = new TeamEntity { Name = ".NET" };
        Db.Teams.AddRange(team1, team2);
        await Db.SaveChangesAsync();

        await SeedConsultant("Lea", "Net", team1.Id, DateTime.UtcNow.AddDays(-1));
        await SeedConsultant("Sander", "Java", team2.Id, DateTime.UtcNow.AddDays(-5));

        _user.IsBackOffice.Returns(false);
        _user.Teams.Returns([team1.Id]);

        var result = await _sut.GetConsultants();

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var consultants = ok!.Value as IReadOnlyList<ConsultantSummaryDto>;
        Assert.That(consultants, Has.Count.EqualTo(1));
        Assert.That(consultants![0].DisplayName, Is.EqualTo("Lea Net"));
    }

    [Test]
    public async Task GetConsultants_WhenBackOffice_ReturnsAllConsultants()
    {
        var team1 = new TeamEntity { Name = "Java" };
        var team2 = new TeamEntity { Name = ".NET" };
        Db.Teams.AddRange(team1, team2);
        await Db.SaveChangesAsync();

        await SeedConsultant("Lea", "Net", team1.Id);
        await SeedConsultant("Sander", "Java", team2.Id);

        _user.IsBackOffice.Returns(true);

        var result = await _sut.GetConsultants();

        var ok = result.Result as OkObjectResult;
        var consultants = ok!.Value as IReadOnlyList<ConsultantSummaryDto>;
        Assert.That(consultants, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetConsultants_IncludesTeamName()
    {
        var team = new TeamEntity { Name = ".NET" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        await SeedConsultant("Lea", "Van Den Berg", team.Id);

        _user.IsBackOffice.Returns(true);

        var result = await _sut.GetConsultants();

        var ok = result.Result as OkObjectResult;
        var consultants = ok!.Value as IReadOnlyList<ConsultantSummaryDto>;
        Assert.That(consultants![0].TeamName, Is.EqualTo(".NET"));
    }

    [Test]
    public async Task GetConsultants_FlagsInactiveConsultants()
    {
        var team = new TeamEntity { Name = ".NET" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        await SeedConsultant("Active", "User", team.Id, DateTime.UtcNow.AddDays(-5));
        await SeedConsultant("Inactive", "User", team.Id, DateTime.UtcNow.AddDays(-25));
        await SeedConsultant("Never", "Active", team.Id, lastActivityAt: null);

        _user.IsBackOffice.Returns(true);

        var result = await _sut.GetConsultants();

        var ok = result.Result as OkObjectResult;
        var consultants = (ok!.Value as IReadOnlyList<ConsultantSummaryDto>)!;
        Assert.That(consultants.Count(c => c.IsInactive), Is.EqualTo(2));
        Assert.That(consultants.Count(c => !c.IsInactive), Is.EqualTo(1));
    }

    [Test]
    public async Task GetConsultants_CalculatesDaysSinceActivity()
    {
        var team = new TeamEntity { Name = ".NET" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        await SeedConsultant("Lea", "User", team.Id, DateTime.UtcNow.AddDays(-10));

        _user.IsBackOffice.Returns(true);

        var result = await _sut.GetConsultants();

        var ok = result.Result as OkObjectResult;
        var consultants = ok!.Value as IReadOnlyList<ConsultantSummaryDto>;
        Assert.That(consultants![0].DaysSinceActivity, Is.EqualTo(10).Within(1));
    }

    [Test]
    public async Task GetConsultant_WhenExists_ReturnsDetail()
    {
        var team = new TeamEntity { Name = ".NET" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        var (_, userId) = await SeedConsultant("Lea", "Van Den Berg", team.Id, DateTime.UtcNow.AddDays(-3));

        _user.IsBackOffice.Returns(true);

        var result = await _sut.GetConsultant(userId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as ConsultantDetailDto;
        Assert.That(dto!.UserId, Is.EqualTo(userId));
        Assert.That(dto.DisplayName, Is.EqualTo("Lea Van Den Berg"));
        Assert.That(dto.Email, Is.EqualTo("lea@test.local"));
        Assert.That(dto.TeamName, Is.EqualTo(".NET"));
    }

    [Test]
    public async Task GetConsultant_WhenNotExists_ReturnsNotFound()
    {
        _user.IsBackOffice.Returns(true);

        var result = await _sut.GetConsultant("nonexistent-id");

        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetConsultant_WhenManagerAccessingOtherTeam_ReturnsNotFound()
    {
        var team1 = new TeamEntity { Name = "Java" };
        var team2 = new TeamEntity { Name = ".NET" };
        Db.Teams.AddRange(team1, team2);
        await Db.SaveChangesAsync();

        var (_, userId) = await SeedConsultant("Lea", "User", team2.Id);

        _user.IsBackOffice.Returns(false);
        _user.Teams.Returns([team1.Id]); // manager only has team1

        var result = await _sut.GetConsultant(userId);

        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task GetConsultants_WhenManagerHasNoTeam_ReturnsEmpty()
    {
        _user.IsBackOffice.Returns(false);
        _user.Teams.Returns(Array.Empty<int>());

        var result = await _sut.GetConsultants();

        var ok = result.Result as OkObjectResult;
        var consultants = ok!.Value as IReadOnlyList<ConsultantSummaryDto>;
        Assert.That(consultants, Is.Empty);
    }
}
