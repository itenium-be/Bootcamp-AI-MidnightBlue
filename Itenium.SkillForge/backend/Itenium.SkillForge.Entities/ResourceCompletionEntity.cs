using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

/// <summary>
/// Records that a consultant has completed a resource (SF-16).
/// One record per (ConsultantId, ResourceId) — unique constraint enforced in AppDbContext.
/// </summary>
public class ResourceCompletionEntity
{
    [Key]
    public int Id { get; set; }

    /// <summary>Username of the consultant who completed the resource.</summary>
    [Required]
    [MaxLength(200)]
    public required string ConsultantId { get; set; }

    public int ResourceId { get; set; }
    public ResourceEntity Resource { get; set; } = null!;

    /// <summary>Linked goal (nullable — SF-11 will populate this once goals are implemented).</summary>
    public int? GoalId { get; set; }

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
