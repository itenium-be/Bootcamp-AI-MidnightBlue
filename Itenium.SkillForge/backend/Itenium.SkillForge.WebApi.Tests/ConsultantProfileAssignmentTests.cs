using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

[TestFixture]
public class ConsultantProfileAssignmentTests : DatabaseTestBase
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
        DateTime? lastActivityAt = null,
        int? profileId = null)
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
            ProfileId = profileId,
        });
        await Db.SaveChangesAsync();
        return (team, userId);
    }

    [Test]
    public async Task AssignProfile_WhenBackOffice_Returns204()
    {
        var team = new TeamEntity { Name = ".NET" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        var (_, userId) = await SeedConsultant("Lea", "Net", team.Id);

        var profile = new CompetenceCentreProfileEntity { Name = "DotNet Profile" };
        Db.CompetenceCentreProfiles.Add(profile);
        await Db.SaveChangesAsync();

        _user.IsBackOffice.Returns(true);

        var result = await _sut.AssignProfile(userId, new AssignProfileRequest(profile.Id));

        Assert.That(result, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task AssignProfile_WhenManagerOnSameTeam_Returns204()
    {
        var team = new TeamEntity { Name = ".NET" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        var (_, userId) = await SeedConsultant("Lea", "Net", team.Id);

        var profile = new CompetenceCentreProfileEntity { Name = "DotNet Profile" };
        Db.CompetenceCentreProfiles.Add(profile);
        await Db.SaveChangesAsync();

        _user.IsBackOffice.Returns(false);
        _user.Teams.Returns([team.Id]);

        var result = await _sut.AssignProfile(userId, new AssignProfileRequest(profile.Id));

        Assert.That(result, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task AssignProfile_WhenManagerOnDifferentTeam_Returns404()
    {
        var team1 = new TeamEntity { Name = "Java" };
        var team2 = new TeamEntity { Name = ".NET" };
        Db.Teams.AddRange(team1, team2);
        await Db.SaveChangesAsync();

        var (_, userId) = await SeedConsultant("Lea", "Net", team2.Id);

        var profile = new CompetenceCentreProfileEntity { Name = "DotNet Profile" };
        Db.CompetenceCentreProfiles.Add(profile);
        await Db.SaveChangesAsync();

        _user.IsBackOffice.Returns(false);
        _user.Teams.Returns([team1.Id]); // manager only has team1

        var result = await _sut.AssignProfile(userId, new AssignProfileRequest(profile.Id));

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task AssignProfile_WhenConsultantNotFound_Returns404()
    {
        _user.IsBackOffice.Returns(true);

        var result = await _sut.AssignProfile("nonexistent-id", new AssignProfileRequest(1));

        Assert.That(result, Is.TypeOf<NotFoundResult>());
    }

    [Test]
    public async Task AssignProfile_WithNullProfileId_ClearsProfile()
    {
        var team = new TeamEntity { Name = ".NET" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        var profile = new CompetenceCentreProfileEntity { Name = "DotNet Profile" };
        Db.CompetenceCentreProfiles.Add(profile);
        await Db.SaveChangesAsync();

        var (_, userId) = await SeedConsultant("Lea", "Net", team.Id, profileId: profile.Id);

        _user.IsBackOffice.Returns(true);

        var result = await _sut.AssignProfile(userId, new AssignProfileRequest(null));

        Assert.That(result, Is.TypeOf<NoContentResult>());

        var consultantProfile = await Db.ConsultantProfiles.FindAsync(userId);
        Assert.That(consultantProfile!.ProfileId, Is.Null);
    }

    [Test]
    public async Task GetConsultant_IncludesProfileInfo_WhenProfileAssigned()
    {
        var team = new TeamEntity { Name = ".NET" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        var profile = new CompetenceCentreProfileEntity { Name = "DotNet Profile" };
        Db.CompetenceCentreProfiles.Add(profile);
        await Db.SaveChangesAsync();

        var (_, userId) = await SeedConsultant("Lea", "Net", team.Id, profileId: profile.Id);

        _user.IsBackOffice.Returns(true);

        var result = await _sut.GetConsultant(userId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var dto = ok!.Value as ConsultantDetailDto;
        Assert.That(dto!.ProfileId, Is.EqualTo(profile.Id));
        Assert.That(dto.ProfileName, Is.EqualTo("DotNet Profile"));
    }

    [Test]
    public async Task GetConsultantSkills_WhenProfileAssigned_ReturnsSkillsGroupedByCategory()
    {
        var team = new TeamEntity { Name = ".NET" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        var profile = new CompetenceCentreProfileEntity { Name = "DotNet Profile" };
        Db.CompetenceCentreProfiles.Add(profile);
        await Db.SaveChangesAsync();

        var skill1 = new SkillEntity { Name = "C# Language", Category = ".NET", LevelCount = 5 };
        var skill2 = new SkillEntity { Name = "Clean Code", Category = "Craftsmanship", LevelCount = 3 };
        Db.Skills.AddRange(skill1, skill2);
        await Db.SaveChangesAsync();

        Db.CompetenceCentreProfileSkills.AddRange(
            new CompetenceCentreProfileSkillEntity { ProfileId = profile.Id, SkillId = skill1.Id },
            new CompetenceCentreProfileSkillEntity { ProfileId = profile.Id, SkillId = skill2.Id }
        );
        await Db.SaveChangesAsync();

        var (_, userId) = await SeedConsultant("Lea", "Net", team.Id, profileId: profile.Id);

        _user.IsBackOffice.Returns(true);

        var result = await _sut.GetConsultantSkills(userId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var categories = ok!.Value as IReadOnlyList<RoadmapCategoryDto>;
        Assert.That(categories, Is.Not.Null);
        Assert.That(categories!.Count, Is.EqualTo(2));

        // categories are ordered by name: ".NET" before "Craftsmanship"
        Assert.That(categories[0].Category, Is.EqualTo(".NET"));
        Assert.That(categories[1].Category, Is.EqualTo("Craftsmanship"));
        Assert.That(categories[0].Skills, Has.Count.EqualTo(1));
        Assert.That(categories[0].Skills[0].Name, Is.EqualTo("C# Language"));
    }

    [Test]
    public async Task GetConsultantSkills_WhenNoProfileAssigned_ReturnsEmpty()
    {
        var team = new TeamEntity { Name = ".NET" };
        Db.Teams.Add(team);
        await Db.SaveChangesAsync();

        var (_, userId) = await SeedConsultant("Lea", "Net", team.Id, profileId: null);

        _user.IsBackOffice.Returns(true);

        var result = await _sut.GetConsultantSkills(userId);

        var ok = result.Result as OkObjectResult;
        Assert.That(ok, Is.Not.Null);
        var items = ok!.Value as RoadmapCategoryDto[];
        Assert.That(items, Is.Empty);
    }

    [Test]
    public async Task GetConsultantSkills_WhenManagerOnDifferentTeam_ReturnsNotFound()
    {
        var team1 = new TeamEntity { Name = "Java" };
        var team2 = new TeamEntity { Name = ".NET" };
        Db.Teams.AddRange(team1, team2);
        await Db.SaveChangesAsync();

        var (_, userId) = await SeedConsultant("Lea", "Net", team2.Id);

        _user.IsBackOffice.Returns(false);
        _user.Teams.Returns([team1.Id]);

        var result = await _sut.GetConsultantSkills(userId);

        Assert.That(result.Result, Is.TypeOf<NotFoundResult>());
    }
}
