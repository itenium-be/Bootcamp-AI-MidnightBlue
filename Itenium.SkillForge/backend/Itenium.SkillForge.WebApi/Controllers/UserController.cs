using System.Globalization;
using System.Security.Claims;
using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Data;
using Itenium.SkillForge.Services;
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
    private static readonly DateTimeOffset ArchivedSentinel = new(2099, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly UserManager<ForgeUser> _userManager;
    private readonly AppDbContext _db;
    private readonly ISkillForgeUser _currentUser;

    public UserController(UserManager<ForgeUser> userManager, AppDbContext db, ISkillForgeUser currentUser)
    {
        _userManager = userManager;
        _db = db;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Get all active (non-archived) users. Backoffice only.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        var users = await _userManager.Users
            .Where(u => !u.LockoutEnd.HasValue || u.LockoutEnd < ArchivedSentinel)
            .ToListAsync();

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

    /// <summary>
    /// Archive (soft-delete) a user account. Backoffice only.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> ArchiveUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var claims = await _userManager.GetClaimsAsync(user);
        if (claims.Any(c => c.Type == "archived-at"))
        {
            return BadRequest("User is already archived.");
        }

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, ArchivedSentinel);
        await _userManager.AddClaimAsync(user, new Claim("archived-at", DateTime.UtcNow.ToString("O")));
        await _userManager.AddClaimAsync(user, new Claim("archived-by", _currentUser.UserId ?? "system"));

        return NoContent();
    }

    /// <summary>
    /// Get all archived users. Backoffice only.
    /// </summary>
    [HttpGet("archived")]
    public async Task<ActionResult<List<ArchivedUserResponse>>> GetArchivedUsers()
    {
        var archivedUsers = await _userManager.Users
            .Where(u => u.LockoutEnd.HasValue && u.LockoutEnd >= ArchivedSentinel)
            .ToListAsync();

        var result = new List<ArchivedUserResponse>();
        foreach (var user in archivedUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);
            var teamIds = claims
                .Where(c => c.Type == "team")
                .Select(c => int.Parse(c.Value, CultureInfo.InvariantCulture))
                .ToArray();

            var archivedAtStr = claims.FirstOrDefault(c => c.Type == "archived-at")?.Value;
            var archivedBy = claims.FirstOrDefault(c => c.Type == "archived-by")?.Value ?? "";

            if (!DateTime.TryParse(archivedAtStr, null, DateTimeStyles.RoundtripKind, out var archivedAt))
            {
                archivedAt = DateTime.UtcNow;
            }

            result.Add(new ArchivedUserResponse(
                user.Id,
                user.FirstName ?? "",
                user.LastName ?? "",
                user.Email ?? "",
                roles.FirstOrDefault() ?? "",
                teamIds,
                archivedAt,
                archivedBy));
        }

        return Ok(result);
    }

    /// <summary>
    /// Restore an archived user account. Backoffice only.
    /// </summary>
    [HttpPost("{id}/restore")]
    public async Task<ActionResult<UserResponse>> RestoreUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var claims = await _userManager.GetClaimsAsync(user);
        var archivedAtClaim = claims.FirstOrDefault(c => c.Type == "archived-at");
        if (archivedAtClaim == null)
        {
            return BadRequest("User is not archived.");
        }

        var archivedByClaim = claims.FirstOrDefault(c => c.Type == "archived-by");

        await _userManager.RemoveClaimAsync(user, archivedAtClaim);
        if (archivedByClaim != null)
        {
            await _userManager.RemoveClaimAsync(user, archivedByClaim);
        }

        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.SetLockoutEnabledAsync(user, false);

        var roles = await _userManager.GetRolesAsync(user);
        var remainingClaims = await _userManager.GetClaimsAsync(user);
        var teamIds = remainingClaims
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
    /// Get consultants (learners) who have no active coach on their team. Backoffice only.
    /// </summary>
    [HttpGet("uncoached")]
    public async Task<ActionResult<List<UserResponse>>> GetUncoachedUsers()
    {
        var activeUsers = await _userManager.Users
            .Where(u => !u.LockoutEnd.HasValue || u.LockoutEnd < ArchivedSentinel)
            .ToListAsync();

        // Collect all team IDs that have at least one active manager
        var coachedTeamIds = new HashSet<int>();
        foreach (var user in activeUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("manager", StringComparer.Ordinal))
            {
                continue;
            }

            var claims = await _userManager.GetClaimsAsync(user);
            foreach (var teamId in claims
                .Where(c => c.Type == "team")
                .Select(c => int.Parse(c.Value, CultureInfo.InvariantCulture)))
            {
                coachedTeamIds.Add(teamId);
            }
        }

        // Return learners not covered by any active manager
        var result = new List<UserResponse>();
        foreach (var user in activeUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("learner", StringComparer.Ordinal))
            {
                continue;
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var teamIds = claims
                .Where(c => c.Type == "team")
                .Select(c => int.Parse(c.Value, CultureInfo.InvariantCulture))
                .ToArray();

            if (!teamIds.Any(t => coachedTeamIds.Contains(t)))
            {
                result.Add(new UserResponse(
                    user.Id,
                    user.FirstName ?? "",
                    user.LastName ?? "",
                    user.Email ?? "",
                    "learner",
                    teamIds));
            }
        }

        return Ok(result);
    }
}
