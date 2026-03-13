using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SkillController : ControllerBase
{
    private readonly AppDbContext _db;

    public SkillController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Get all skills.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<SkillEntity>>> GetSkills()
    {
        var skills = await _db.Skills.ToListAsync();
        return Ok(skills);
    }

    /// <summary>
    /// Get a skill by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SkillEntity>> GetSkill(int id)
    {
        var skill = await _db.Skills.FindAsync(id);
        if (skill == null) return NotFound();
        return Ok(skill);
    }

    /// <summary>
    /// Create a new skill.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SkillEntity>> CreateSkill([FromBody] CreateSkillRequest request)
    {
        var skill = new SkillEntity
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            LevelCount = request.LevelCount,
            LevelDescriptors = request.LevelDescriptors ?? [],
        };

        _db.Skills.Add(skill);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSkill), new { id = skill.Id }, skill);
    }

    /// <summary>
    /// Update an existing skill.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<SkillEntity>> UpdateSkill(int id, [FromBody] UpdateSkillRequest request)
    {
        var skill = await _db.Skills.FindAsync(id);
        if (skill == null) return NotFound();

        skill.Name = request.Name;
        skill.Description = request.Description;
        skill.Category = request.Category;
        skill.LevelCount = request.LevelCount;
        skill.LevelDescriptors = request.LevelDescriptors ?? [];

        await _db.SaveChangesAsync();

        return Ok(skill);
    }

    /// <summary>
    /// Delete a skill.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteSkill(int id)
    {
        var skill = await _db.Skills.FindAsync(id);
        if (skill == null) return NotFound();

        _db.Skills.Remove(skill);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
