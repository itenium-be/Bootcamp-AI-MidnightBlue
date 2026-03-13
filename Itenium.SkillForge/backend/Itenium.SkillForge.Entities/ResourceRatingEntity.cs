using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

public class ResourceRatingEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string ConsultantId { get; set; }

    public int ResourceId { get; set; }
    public ResourceEntity Resource { get; set; } = null!;

    public bool IsUpvote { get; set; }

    public DateTime RatedAt { get; set; } = DateTime.UtcNow;
}
