using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

/// <summary>
/// A skill in the global catalogue. LevelCount=1 means checkbox (present/absent),
/// 2–7 means a progression with distinct level descriptors.
/// </summary>
public class SkillEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Category { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// 1 = checkbox (no progression), 2–7 = progression levels.
    /// </summary>
    public int LevelCount { get; set; } = 3;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SkillLevelDescriptorEntity> LevelDescriptors { get; set; } = [];

    /// <summary>
    /// Skills this skill requires (prerequisites this skill depends on).
    /// </summary>
    public ICollection<SkillPrerequisiteEntity> Prerequisites { get; set; } = [];

    /// <summary>
    /// Skills that list this skill as a prerequisite (dependents).
    /// </summary>
    public ICollection<SkillPrerequisiteEntity> Dependents { get; set; } = [];

    public ICollection<CompetenceCentreProfileSkillEntity> ProfileSkills { get; set; } = [];

    public override string ToString() => $"{Name} ({Category})";
}
