using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

public class GoalEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string ConsultantId { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    public int? SkillId { get; set; }

    public int? TargetLevel { get; set; }

    public DateTime? Deadline { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [MaxLength(200)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
