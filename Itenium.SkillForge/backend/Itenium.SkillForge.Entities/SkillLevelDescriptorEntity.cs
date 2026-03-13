using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

/// <summary>
/// A descriptor for one niveau of a skill's progression.
/// Level ranges from 1 to SkillEntity.LevelCount.
/// </summary>
public class SkillLevelDescriptorEntity
{
    [Key]
    public int Id { get; set; }

    public int SkillId { get; set; }
    public SkillEntity Skill { get; set; } = null!;

    /// <summary>
    /// Niveau number, 1–7. For checkbox skills (LevelCount=1) there is exactly one descriptor at Level=1.
    /// </summary>
    public int Level { get; set; }

    [Required]
    [MaxLength(1000)]
    public required string Description { get; set; }
}
