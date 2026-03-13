using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

/// <summary>
/// A learning resource (article, video, book, etc.) linked to a skill.
/// </summary>
public class ResourceEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(500)]
    public required string Url { get; set; }

    public ResourceType Type { get; set; }

    /// <summary>Skill this resource is linked to (references CourseEntity.Id until SF-01 renames it to SkillEntity).</summary>
    public int? SkillId { get; set; }

    /// <summary>Minimum skill niveau this resource targets.</summary>
    public int? FromLevel { get; set; }

    /// <summary>Maximum skill niveau this resource targets.</summary>
    public int? ToLevel { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>Upvote count — updated by SF-17 (Rate a resource).</summary>
    public int Upvotes { get; set; }

    /// <summary>Downvote count — updated by SF-17 (Rate a resource).</summary>
    public int Downvotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public override string ToString() => $"{Title} ({Type})";
}
