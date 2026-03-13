using System.Text.Json.Serialization;

namespace Itenium.SkillForge.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResourceType
{
    Article,
    Video,
    Book,
    Course,
    Other,
}
