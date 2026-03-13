namespace Itenium.SkillForge.Entities;

/// <summary>
/// Expresses that a skill requires another skill at a minimum level as a prerequisite.
/// Non-blocking: presence of unmet prerequisites triggers a warning, not a lock.
/// </summary>
public class SkillPrerequisiteEntity
{
    /// <summary>
    /// The skill that declares the prerequisite (the dependent).
    /// </summary>
    public int SkillId { get; set; }
    public SkillEntity Skill { get; set; } = null!;

    /// <summary>
    /// The skill that must be met (the prerequisite).
    /// </summary>
    public int RequiredSkillId { get; set; }
    public SkillEntity RequiredSkill { get; set; } = null!;

    /// <summary>
    /// Minimum niveau that must be reached on the required skill.
    /// </summary>
    public int RequiredLevel { get; set; } = 1;
}
