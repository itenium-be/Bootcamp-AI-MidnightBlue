using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResourceController : ControllerBase
{
    private readonly AppDbContext _db;

    public ResourceController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Browse the resource library. Optionally filter by skill and/or type.
    /// Accessible by all authenticated users (learner, manager, backoffice).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ResourceEntity>>> GetResources(
        [FromQuery] int? skillId,
        [FromQuery] ResourceType? type)
    {
        var query = _db.Resources.AsQueryable();

        if (skillId.HasValue)
            query = query.Where(r => r.SkillId == skillId.Value);

        if (type.HasValue)
            query = query.Where(r => r.Type == type.Value);

        var resources = await query.OrderBy(r => r.Title).ToListAsync();
        return Ok(resources);
    }

    /// <summary>
    /// Contribute a new resource to the library.
    /// Accessible by all authenticated users (learner, manager, backoffice).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ResourceEntity>> ContributeResource([FromBody] ContributeResourceRequest request)
    {
        var resource = new ResourceEntity
        {
            Title = request.Title,
            Url = request.Url,
            Type = request.Type,
            SkillId = request.SkillId,
            FromLevel = request.FromLevel,
            ToLevel = request.ToLevel,
            Description = request.Description,
            ContributedBy = User.Identity?.Name,
            ContributedAt = DateTime.UtcNow,
        };

        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetResources), new { id = resource.Id }, resource);
    }

    /// <summary>
    /// Get the IDs of resources the current user has completed.
    /// </summary>
    [HttpGet("completions")]
    public async Task<ActionResult<List<int>>> GetMyCompletions()
    {
        var consultantId = User.Identity?.Name;
        var ids = await _db.ResourceCompletions
            .Where(c => c.ConsultantId == consultantId)
            .Select(c => c.ResourceId)
            .ToListAsync();
        return Ok(ids);
    }

    /// <summary>
    /// Mark a resource as completed. Idempotent — returns the existing record if already completed.
    /// Accessible by all authenticated users (learner, manager, backoffice).
    /// </summary>
    [HttpPost("{id:int}/complete")]
    public async Task<ActionResult<ResourceCompletionEntity>> MarkCompleted(int id)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource == null)
            return NotFound();

        var consultantId = User.Identity?.Name;

        var existing = await _db.ResourceCompletions
            .FirstOrDefaultAsync(c => c.ConsultantId == consultantId && c.ResourceId == id);

        if (existing != null)
            return Ok(existing);

        var completion = new ResourceCompletionEntity
        {
            ConsultantId = consultantId!,
            ResourceId = id,
        };

        _db.ResourceCompletions.Add(completion);
        await _db.SaveChangesAsync();

        return Ok(completion);
    }
}
