using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GoalController : ControllerBase
{
    private readonly AppDbContext _db;

    public GoalController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get the current consultant's goals, including readiness flag status.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<GoalDto>>> GetMyGoals()
    {
        var consultantId = User.Identity?.Name;
        var goals = await _db.Goals
            .Where(g => g.ConsultantId == consultantId)
            .OrderBy(g => g.CreatedAt)
            .ToListAsync();

        var goalIds = goals.Select(g => g.Id).ToList();
        var flags = await _db.ReadinessFlags
            .Where(f => goalIds.Contains(f.GoalId))
            .ToDictionaryAsync(f => f.GoalId, f => f.RaisedAt);

        var dtos = goals.Select(g => new GoalDto(
            g.Id,
            g.Title,
            g.SkillId,
            g.TargetLevel,
            g.Deadline,
            g.Notes,
            g.CreatedBy,
            g.CreatedAt,
            flags.TryGetValue(g.Id, out var raisedAt) ? raisedAt : null)).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Raise a readiness flag on a goal. Idempotent — returns existing flag if already raised.
    /// One active flag per goal is enforced.
    /// </summary>
    [HttpPost("{id:int}/ready")]
    public async Task<ActionResult<ReadinessFlagEntity>> RaiseReadinessFlag(int id)
    {
        var consultantId = User.Identity?.Name;
        var goal = await _db.Goals.FirstOrDefaultAsync(g => g.Id == id && g.ConsultantId == consultantId);
        if (goal == null)
            return NotFound();

        var existing = await _db.ReadinessFlags.FirstOrDefaultAsync(f => f.GoalId == id);
        if (existing != null)
            return Ok(existing);

        var flag = new ReadinessFlagEntity
        {
            GoalId = id,
            ConsultantId = consultantId!,
        };

        _db.ReadinessFlags.Add(flag);
        await _db.SaveChangesAsync();
        return Ok(flag);
    }

    /// <summary>
    /// Get all active readiness flags across the team (for coaches).
    /// Returns flags ordered by age (oldest first).
    /// </summary>
    [HttpGet("flags")]
    public async Task<ActionResult<List<TeamFlagDto>>> GetTeamFlags()
    {
        var flags = await _db.ReadinessFlags
            .Include(f => f.Goal)
            .OrderBy(f => f.RaisedAt)
            .Select(f => new TeamFlagDto(f.GoalId, f.Goal.Title, f.ConsultantId, f.RaisedAt))
            .ToListAsync();

        return Ok(flags);
    }
}
