using Itenium.SkillForge.Data;
using Itenium.SkillForge.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConsultantController : ControllerBase
{
    private static readonly TimeSpan InactivityThreshold = TimeSpan.FromDays(21);

    private readonly AppDbContext _db;
    private readonly ISkillForgeUser _user;

    public ConsultantController(AppDbContext db, ISkillForgeUser user)
    {
        _db = db;
        _user = user;
    }

    /// <summary>
    /// Get all consultants visible to the current user.
    /// Managers see their own team(s); backoffice sees all.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConsultantSummaryDto>>> GetConsultants()
    {
        var now = DateTime.UtcNow;

        var query = _db.ConsultantProfiles
            .Include(p => p.Team)
            .AsQueryable();

        if (!_user.IsBackOffice)
        {
            var teamIds = _user.Teams;
            query = query.Where(p => teamIds.Contains(p.TeamId));
        }

        var profiles = await query
            .OrderBy(p => p.Team.Name)
            .ThenBy(p => p.UserId)
            .ToListAsync();

        var userIds = profiles.Select(p => p.UserId).ToList();
        var users = await _db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var result = profiles
            .Where(p => users.ContainsKey(p.UserId))
            .Select(p =>
            {
                var u = users[p.UserId];
                var days = p.LastActivityAt.HasValue
                    ? (int)(now - p.LastActivityAt.Value).TotalDays
                    : (int?)null;
                var inactive = p.LastActivityAt == null || (now - p.LastActivityAt.Value) > InactivityThreshold;
                return new ConsultantSummaryDto(
                    p.UserId,
                    $"{u.FirstName} {u.LastName}",
                    u.Email ?? string.Empty,
                    p.TeamId,
                    p.Team.Name,
                    p.LastActivityAt,
                    inactive,
                    days);
            })
            .ToList();

        return Ok(result);
    }

    /// <summary>
    /// Get a single consultant's profile. Managers can only access their own team.
    /// </summary>
    [HttpGet("{userId}")]
    public async Task<ActionResult<ConsultantDetailDto>> GetConsultant(string userId)
    {
        var profile = await _db.ConsultantProfiles
            .Include(p => p.Team)
            .Include(p => p.Profile)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            return NotFound();
        }

        if (!_user.IsBackOffice && !_user.Teams.Contains(profile.TeamId))
        {
            return NotFound();
        }

        var identityUser = await _db.Users.FindAsync(userId);
        if (identityUser == null)
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;
        var days = profile.LastActivityAt.HasValue
            ? (int)(now - profile.LastActivityAt.Value).TotalDays
            : (int?)null;
        var inactive = profile.LastActivityAt == null || (now - profile.LastActivityAt.Value) > InactivityThreshold;

        return Ok(new ConsultantDetailDto(
            profile.UserId,
            $"{identityUser.FirstName} {identityUser.LastName}",
            identityUser.Email ?? string.Empty,
            profile.TeamId,
            profile.Team.Name,
            profile.LastActivityAt,
            inactive,
            days,
            profile.CreatedAt,
            profile.ProfileId,
            profile.Profile?.Name));
    }

    /// <summary>
    /// Assign or clear a competence centre profile for a consultant.
    /// Only managers on the same team or backoffice can assign profiles.
    /// </summary>
    [HttpPut("{userId}/profile")]
    [Authorize(Roles = "manager,backoffice")]
    public async Task<IActionResult> AssignProfile(string userId, [FromBody] AssignProfileRequest request)
    {
        var profile = await _db.ConsultantProfiles.Include(p => p.Team).FirstOrDefaultAsync(p => p.UserId == userId);
        if (profile == null) return NotFound();
        if (!_user.IsBackOffice && !_user.Teams.Contains(profile.TeamId)) return NotFound();

        if (request.ProfileId.HasValue)
        {
            var exists = await _db.CompetenceCentreProfiles.AnyAsync(p => p.Id == request.ProfileId.Value);
            if (!exists) return BadRequest("Profile not found.");
        }

        profile.ProfileId = request.ProfileId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Get the roadmap skills for a consultant filtered by their assigned competence centre profile.
    /// Each skill is annotated with any unmet prerequisites based on the consultant's current skill levels.
    /// Skills with unmet prerequisites are warned (never locked).
    /// </summary>
    [HttpGet("{userId}/skills")]
    public async Task<ActionResult<IReadOnlyList<RoadmapCategoryDto>>> GetConsultantSkills(string userId)
    {
        var consultantProfile = await _db.ConsultantProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (consultantProfile == null) return NotFound();
        if (!_user.IsBackOffice && !_user.Teams.Contains(consultantProfile.TeamId)) return NotFound();

        if (consultantProfile.ProfileId == null) return Ok(Array.Empty<RoadmapCategoryDto>());

        var skills = await _db.CompetenceCentreProfileSkills
            .Where(ps => ps.ProfileId == consultantProfile.ProfileId)
            .Include(ps => ps.Skill)
                .ThenInclude(s => s.Prerequisites)
                    .ThenInclude(p => p.RequiredSkill)
            .Select(ps => ps.Skill)
            .ToListAsync();

        var currentLevels = await _db.ConsultantSkillLevels
            .Where(l => l.UserId == userId)
            .ToDictionaryAsync(l => l.SkillId, l => l.CurrentLevel);

        var roadmapSkills = skills.Select(skill =>
        {
            var unmetPrereqs = skill.Prerequisites
                .Where(p => currentLevels.GetValueOrDefault(p.RequiredSkillId, 0) < p.RequiredLevel)
                .Select(p => new SkillPrerequisiteDto(p.RequiredSkillId, p.RequiredSkill.Name, p.RequiredLevel))
                .ToList();

            return new RoadmapSkillDto(skill.Id, skill.Name, skill.Category, skill.Description, skill.LevelCount, unmetPrereqs);
        }).ToList();

        var categories = roadmapSkills
            .GroupBy(s => s.Category, StringComparer.Ordinal)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => new RoadmapCategoryDto(g.Key, g.OrderBy(s => s.Name, StringComparer.Ordinal).ToList()))
            .ToList();

        return Ok(categories);
    }
}
