namespace Itenium.SkillForge.Entities;

/// <summary>
/// Tracks the current validated skill level for a consultant on a specific skill.
/// Used to evaluate prerequisite warnings on the roadmap.
/// </summary>
public class ConsultantSkillLevelEntity
{
    public string UserId { get; set; } = null!;
    public int SkillId { get; set; }

    /// <summary>
    /// The current niveau reached (1–N). Defaults to 0 (not started).
    /// </summary>
    public int CurrentLevel { get; set; }

    public SkillEntity Skill { get; set; } = null!;
}