namespace Itenium.SkillForge.WebApi.Controllers;

public record SkillSummaryDto(
    int Id,
    string Name,
    string Category,
    string? Description,
    int LevelCount
);

public record SkillLevelDescriptorDto(int Level, string Description);

public record SkillPrerequisiteDto(int RequiredSkillId, string RequiredSkillName, int RequiredLevel);

public record SkillDetailDto(
    int Id,
    string Name,
    string Category,
    string? Description,
    int LevelCount,
    IReadOnlyList<SkillLevelDescriptorDto> LevelDescriptors,
    IReadOnlyList<SkillPrerequisiteDto> Prerequisites
);

public record SkillCategoryDto(string Category, IReadOnlyList<SkillSummaryDto> Skills);

public record CompetenceCentreProfileSummaryDto(int Id, string Name, string? Description, int SkillCount);
