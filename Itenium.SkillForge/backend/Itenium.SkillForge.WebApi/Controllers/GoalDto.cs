namespace Itenium.SkillForge.WebApi.Controllers;

public record GoalDto(
    int Id,
    string Title,
    int? SkillId,
    int? TargetLevel,
    DateTime? Deadline,
    string? Notes,
    string? CreatedBy,
    DateTime CreatedAt,
    DateTime? FlagRaisedAt);

public record TeamFlagDto(
    int GoalId,
    string GoalTitle,
    string ConsultantId,
    DateTime RaisedAt);
