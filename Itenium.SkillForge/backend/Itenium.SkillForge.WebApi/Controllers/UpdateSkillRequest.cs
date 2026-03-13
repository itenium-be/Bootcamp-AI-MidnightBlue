namespace Itenium.SkillForge.WebApi.Controllers;

public record UpdateSkillRequest(string Name, string? Description, string? Category, int LevelCount, IList<string>? LevelDescriptors);
