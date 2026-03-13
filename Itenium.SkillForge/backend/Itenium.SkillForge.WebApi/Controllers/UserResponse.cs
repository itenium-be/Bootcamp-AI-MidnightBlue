namespace Itenium.SkillForge.WebApi.Controllers;

public record UserResponse(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    int[] TeamIds);
