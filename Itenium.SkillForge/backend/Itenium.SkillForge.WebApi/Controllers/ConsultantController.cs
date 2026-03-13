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
            profile.CreatedAt));
    }
}
