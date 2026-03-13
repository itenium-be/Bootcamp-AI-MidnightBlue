using Itenium.SkillForge.Data;
using Itenium.SkillForge.Entities;
using Itenium.SkillForge.Services;
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
    private readonly ISkillForgeUser _user;

    public ResourceController(AppDbContext db, ISkillForgeUser user)
    {
        _db = db;
        _user = user;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ResourceDto>>> GetResources()
    {
        var userId = _user.UserId ?? string.Empty;
        var resources = await _db.Resources
            .Include(r => r.Skill)
            .Include(r => r.Completions)
            .Include(r => r.Ratings)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        var result = resources.Select(r => ToDto(r, userId)).ToList();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ResourceDto>> CreateResource([FromBody] CreateResourceRequest request)
    {
        if (request.SkillId.HasValue)
        {
            var skillExists = await _db.Skills.AnyAsync(s => s.Id == request.SkillId.Value);
            if (!skillExists) return BadRequest("Skill not found.");
        }

        var resource = new ResourceEntity
        {
            Title = request.Title,
            Url = request.Url,
            Type = request.Type,
            SkillId = request.SkillId,
            FromLevel = request.FromLevel,
            ToLevel = request.ToLevel,
            AddedByUserId = _user.UserId ?? string.Empty,
        };

        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();

        await _db.Entry(resource).Reference(r => r.Skill).LoadAsync();

        return CreatedAtAction(nameof(GetResources), new { id = resource.Id }, ToDto(resource, _user.UserId ?? string.Empty));
    }

    [HttpPost("{id:int}/complete")]
    public async Task<IActionResult> MarkComplete(int id)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource == null) return NotFound();

        var userId = _user.UserId ?? string.Empty;
        var existing = await _db.ResourceCompletions
            .FirstOrDefaultAsync(c => c.ResourceId == id && c.UserId == userId);

        if (existing == null)
        {
            _db.ResourceCompletions.Add(new ResourceCompletionEntity
            {
                ResourceId = id,
                UserId = userId,
            });
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}/complete")]
    public async Task<IActionResult> UnmarkComplete(int id)
    {
        var userId = _user.UserId ?? string.Empty;
        var existing = await _db.ResourceCompletions
            .FirstOrDefaultAsync(c => c.ResourceId == id && c.UserId == userId);

        if (existing != null)
        {
            _db.ResourceCompletions.Remove(existing);
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpPut("{id:int}/rate")]
    public async Task<IActionResult> RateResource(int id, [FromBody] RateResourceRequest request)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource == null) return NotFound();

        var userId = _user.UserId ?? string.Empty;
        var existing = await _db.ResourceRatings
            .FirstOrDefaultAsync(r => r.ResourceId == id && r.UserId == userId);

        if (existing == null)
        {
            _db.ResourceRatings.Add(new ResourceRatingEntity
            {
                ResourceId = id,
                UserId = userId,
                IsPositive = request.IsPositive,
            });
        }
        else
        {
            existing.IsPositive = request.IsPositive;
            existing.RatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}/rate")]
    public async Task<IActionResult> RemoveRating(int id)
    {
        var userId = _user.UserId ?? string.Empty;
        var existing = await _db.ResourceRatings
            .FirstOrDefaultAsync(r => r.ResourceId == id && r.UserId == userId);

        if (existing != null)
        {
            _db.ResourceRatings.Remove(existing);
            await _db.SaveChangesAsync();
        }

        return NoContent();
    }

    private static ResourceDto ToDto(ResourceEntity r, string userId) => new(
        r.Id,
        r.Title,
        r.Url,
        r.Type,
        r.SkillId,
        r.Skill?.Name,
        r.FromLevel,
        r.ToLevel,
        r.AddedByUserId,
        r.CreatedAt,
        r.Completions.Any(c => c.UserId == userId),
        r.Ratings.FirstOrDefault(x => x.UserId == userId)?.IsPositive,
        r.Ratings.Count(x => x.IsPositive),
        r.Ratings.Count(x => !x.IsPositive)
    );
}
