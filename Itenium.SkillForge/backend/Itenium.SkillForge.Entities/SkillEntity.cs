using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

/// <summary>
/// Skill master data with a variable niveau system (1=checkbox, 2–7=progression).
/// </summary>
public class SkillEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Number of levels (1–7). 1 = checkbox (done/not done), 2–7 = progression.
    /// </summary>
    [Range(1, 7)]
    public int LevelCount { get; set; } = 1;

    /// <summary>
    /// Descriptor text for each niveau, one entry per level (LevelCount entries expected).
    /// </summary>
    public IList<string> LevelDescriptors { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public override string ToString() => $"{Name} ({Category})";
}
