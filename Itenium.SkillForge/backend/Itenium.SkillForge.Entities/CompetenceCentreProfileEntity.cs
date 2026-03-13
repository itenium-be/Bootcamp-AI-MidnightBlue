using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

/// <summary>
/// A competence centre profile (Java, .NET, PO &amp; Analysis, QA) that maps
/// to a subset of the global skill catalogue relevant to consultants in that centre.
/// </summary>
public class CompetenceCentreProfileEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public ICollection<CompetenceCentreProfileSkillEntity> ProfileSkills { get; set; } = [];

    public override string ToString() => Name;
}
