namespace Itenium.SkillForge.WebApi.Controllers;

public record ArchivedUserResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    int[] TeamIds,
    DateTime ArchivedAt,
    string ArchivedBy);
