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
    DateTime CreatedAt,
    int? ProfileId,
    string? ProfileName
);

public record AssignProfileRequest(int? ProfileId);

public record RoadmapSkillDto(
    int Id,
    string Name,
    string Category,
    string? Description,
    int LevelCount,
    IReadOnlyList<SkillPrerequisiteDto> UnmetPrerequisites,
    bool IsInDefaultView
);

public record RoadmapCategoryDto(
    string Category,
    IReadOnlyList<RoadmapSkillDto> Skills
);
