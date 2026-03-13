using System.Globalization;
using System.Security.Claims;
using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Itenium.SkillForge.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "backoffice")]
public class UserController : ControllerBase
{
    private readonly UserManager<ForgeUser> _userManager;
    private readonly AppDbContext _db;

    public UserController(UserManager<ForgeUser> userManager, AppDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    /// <summary>
    /// Get all users. Backoffice only.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var result = new List<UserResponse>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var teamIds = claims
                .Where(c => c.Type == "team")
                .Select(c => int.Parse(c.Value, CultureInfo.InvariantCulture))
                .ToArray();

            result.Add(new UserResponse(
                user.Id,
                user.FirstName ?? "",
                user.LastName ?? "",
                user.Email ?? "",
                roles.FirstOrDefault() ?? "",
                teamIds));
        }

        return Ok(result);
    }

    /// <summary>
    /// Get a user by ID. Backoffice only.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);
        var teamIds = claims
            .Where(c => c.Type == "team")
            .Select(c => int.Parse(c.Value, CultureInfo.InvariantCulture))
            .ToArray();

        return Ok(new UserResponse(
            user.Id,
            user.FirstName ?? "",
            user.LastName ?? "",
            user.Email ?? "",
            roles.FirstOrDefault() ?? "",
            teamIds));
    }

    /// <summary>
    /// Create a new user account. Backoffice only.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        var user = new ForgeUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        foreach (var teamId in request.TeamIds)
        {
            await _userManager.AddClaimAsync(user, new Claim("team", teamId.ToString(CultureInfo.InvariantCulture)));
        }

        var response = new UserResponse(
            user.Id,
            user.FirstName!,
            user.LastName!,
            user.Email!,
            request.Role,
            request.TeamIds);

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, response);
    }
}
