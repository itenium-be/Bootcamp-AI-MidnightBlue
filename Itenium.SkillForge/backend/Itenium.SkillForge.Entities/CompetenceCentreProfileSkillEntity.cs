namespace Itenium.SkillForge.Entities;

/// <summary>
/// Join entity: which skills belong to which competence centre profile.
/// </summary>
public class CompetenceCentreProfileSkillEntity
{
    public int ProfileId { get; set; }
    public CompetenceCentreProfileEntity Profile { get; set; } = null!;

    public int SkillId { get; set; }
    public SkillEntity Skill { get; set; } = null!;
}
