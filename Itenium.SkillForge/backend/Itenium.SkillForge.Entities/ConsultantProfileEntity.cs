using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

/// <summary>
/// Tracks a consultant's team assignment and activity metadata.
/// UserId corresponds to the ASP.NET Identity ForgeUser Id.
/// </summary>
public class ConsultantProfileEntity
{
    [Key]
    [MaxLength(450)]
    public required string UserId { get; set; }

    public int TeamId { get; set; }
    public TeamEntity Team { get; set; } = null!;

    public int? ProfileId { get; set; }
    public CompetenceCentreProfileEntity? Profile { get; set; }

    /// <summary>
    /// Last time the consultant performed any action in the platform.
    /// Null means never active. Used to surface inactive consultants (>21 days).
    /// </summary>
    public DateTime? LastActivityAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
