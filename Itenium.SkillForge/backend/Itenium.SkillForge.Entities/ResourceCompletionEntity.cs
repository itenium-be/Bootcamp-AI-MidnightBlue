using System.ComponentModel.DataAnnotations;
namespace Itenium.SkillForge.Entities;

public class ResourceCompletionEntity
{
    [Key]
    public int Id { get; set; }

    public int ResourceId { get; set; }
    public ResourceEntity Resource { get; set; } = null!;

    [Required]
    [MaxLength(450)]
    public required string UserId { get; set; }

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}
