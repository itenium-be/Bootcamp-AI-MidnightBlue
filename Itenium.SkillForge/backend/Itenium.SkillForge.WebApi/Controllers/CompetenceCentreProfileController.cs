using Itenium.SkillForge.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class CompetenceCentreProfileController : ControllerBase
{
    private readonly AppDbContext _db;

    public CompetenceCentreProfileController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get all competence centre profiles.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CompetenceCentreProfileSummaryDto>>> GetProfiles()
    {
        var profiles = await _db.CompetenceCentreProfiles
            .OrderBy(p => p.Name)
            .Select(p => new CompetenceCentreProfileSummaryDto(
                p.Id,
                p.Name,
                p.Description,
                p.ProfileSkills.Count))
            .ToListAsync();

        return Ok(profiles);
    }

    /// <summary>
    /// Get all skills belonging to a specific competence centre profile.
    /// </summary>
    [HttpGet("{id:int}/skills")]
    public async Task<ActionResult<List<SkillSummaryDto>>> GetProfileSkills(int id)
    {
        var profile = await _db.CompetenceCentreProfiles.FindAsync(id);
        if (profile == null)
        {
            return NotFound();
        }

        var skills = await _db.CompetenceCentreProfileSkills
            .Where(ps => ps.ProfileId == id)
            .Include(ps => ps.Skill)
            .OrderBy(ps => ps.Skill.Category)
            .ThenBy(ps => ps.Skill.Name)
            .Select(ps => new SkillSummaryDto(
                ps.Skill.Id,
                ps.Skill.Name,
                ps.Skill.Category,
                ps.Skill.Description,
                ps.Skill.LevelCount))
            .ToListAsync();

        return Ok(skills);
    }
}
