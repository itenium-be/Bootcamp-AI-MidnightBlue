using System.ComponentModel.DataAnnotations;
namespace Itenium.SkillForge.Entities;

public class ResourceEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string Url { get; set; }

    /// <summary>article | video | book | course | podcast | other</summary>
    [Required]
    [MaxLength(50)]
    public required string Type { get; set; }

    public int? SkillId { get; set; }
    public SkillEntity? Skill { get; set; }

    public int? FromLevel { get; set; }
    public int? ToLevel { get; set; }

    [Required]
    [MaxLength(450)]
    public required string AddedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ResourceCompletionEntity> Completions { get; set; } = [];
    public ICollection<ResourceRatingEntity> Ratings { get; set; } = [];
}
