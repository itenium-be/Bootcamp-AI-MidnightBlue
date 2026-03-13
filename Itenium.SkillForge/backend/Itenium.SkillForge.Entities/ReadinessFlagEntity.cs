using System.ComponentModel.DataAnnotations;

namespace Itenium.SkillForge.Entities;

public class ReadinessFlagEntity
{
    [Key]
    public int Id { get; set; }

    public int GoalId { get; set; }
    public GoalEntity Goal { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public required string ConsultantId { get; set; }

    public DateTime RaisedAt { get; set; } = DateTime.UtcNow;
}
