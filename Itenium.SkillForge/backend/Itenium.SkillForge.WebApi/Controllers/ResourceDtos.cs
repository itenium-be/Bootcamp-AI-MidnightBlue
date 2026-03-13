namespace Itenium.SkillForge.WebApi.Controllers;

public record ResourceDto(
    int Id,
    string Title,
    string Url,
    string Type,
    int? SkillId,
    string? SkillName,
    int? FromLevel,
    int? ToLevel,
    string AddedByUserId,
    DateTime CreatedAt,
    bool CompletedByCurrentUser,
    bool? CurrentUserRating,   // null=not rated, true=👍, false=👎
    int ThumbsUp,
    int ThumbsDown
);

public record CreateResourceRequest(
    string Title,
    string Url,
    string Type,
    int? SkillId,
    int? FromLevel,
    int? ToLevel
);

public record RateResourceRequest(bool IsPositive);
