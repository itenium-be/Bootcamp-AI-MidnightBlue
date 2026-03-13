using System.ComponentModel.DataAnnotations;
namespace Itenium.SkillForge.Entities;

public class ResourceRatingEntity
{
    public int ResourceId { get; set; }
    public ResourceEntity Resource { get; set; } = null!;

    [Required]
    [MaxLength(450)]
    public required string UserId { get; set; }

    public bool IsPositive { get; set; }

    public DateTime RatedAt { get; set; } = DateTime.UtcNow;
}
