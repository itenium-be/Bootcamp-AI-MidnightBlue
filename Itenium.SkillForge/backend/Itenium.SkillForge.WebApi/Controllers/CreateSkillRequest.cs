namespace Itenium.SkillForge.WebApi.Controllers;

public record CreateSkillRequest(string Name, string? Description, string? Category, int LevelCount, IList<string>? LevelDescriptors);
