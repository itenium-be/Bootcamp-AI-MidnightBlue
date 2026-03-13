using Itenium.SkillForge.Data;
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
    /// Get all skills as a flat list.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<SkillSummaryDto>>> GetSkills()
    {
        var skills = await _db.Skills
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .Select(s => new SkillSummaryDto(s.Id, s.Name, s.Category, s.Description, s.LevelCount))
            .ToListAsync();

        return Ok(skills);
    }

    /// <summary>
    /// Get the full skill catalogue grouped by category.
    /// </summary>
    [HttpGet("catalogue")]
    public async Task<ActionResult<List<SkillCategoryDto>>> GetSkillsByCategory()
    {
        var skills = await _db.Skills
            .OrderBy(s => s.Category)
            .ThenBy(s => s.Name)
            .Select(s => new SkillSummaryDto(s.Id, s.Name, s.Category, s.Description, s.LevelCount))
            .ToListAsync();

        var categories = skills
            .GroupBy(s => s.Category, StringComparer.Ordinal)
            .Select(g => new SkillCategoryDto(g.Key, g.ToList()))
            .ToList();

        return Ok(categories);
    }

    /// <summary>
    /// Get a skill by ID including level descriptors and prerequisite links.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SkillDetailDto>> GetSkill(int id)
    {
        var skill = await _db.Skills
            .Include(s => s.LevelDescriptors.OrderBy(d => d.Level))
            .Include(s => s.Prerequisites)
                .ThenInclude(p => p.RequiredSkill)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (skill == null)
        {
            return NotFound();
        }

        var dto = new SkillDetailDto(
            skill.Id,
            skill.Name,
            skill.Category,
            skill.Description,
            skill.LevelCount,
            skill.LevelDescriptors
                .Select(d => new SkillLevelDescriptorDto(d.Level, d.Description))
                .ToList(),
            skill.Prerequisites
                .Select(p => new SkillPrerequisiteDto(p.RequiredSkillId, p.RequiredSkill.Name, p.RequiredLevel))
                .ToList()
        );

        return Ok(dto);
    }
}
