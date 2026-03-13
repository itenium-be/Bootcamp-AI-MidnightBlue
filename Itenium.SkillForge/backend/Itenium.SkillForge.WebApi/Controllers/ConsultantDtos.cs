namespace Itenium.SkillForge.WebApi.Controllers;

public record ConsultantSummaryDto(
    string UserId,
    string DisplayName,
    string Email,
    int TeamId,
    string TeamName,
    DateTime? LastActivityAt,
    bool IsInactive,
    int? DaysSinceActivity
);

public record ConsultantDetailDto(
    string UserId,
    string DisplayName,
    string Email,
    int TeamId,
    string TeamName,
    DateTime? LastActivityAt,
    bool IsInactive,
    int? DaysSinceActivity,
    DateTime CreatedAt
);
