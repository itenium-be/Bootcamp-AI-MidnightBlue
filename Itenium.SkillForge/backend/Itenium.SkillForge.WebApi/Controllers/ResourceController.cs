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
}
