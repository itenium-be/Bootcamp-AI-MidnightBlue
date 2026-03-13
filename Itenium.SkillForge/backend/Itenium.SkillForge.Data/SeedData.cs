using System.Security.Claims;
using Itenium.Forge.Security.OpenIddict;
using Itenium.SkillForge.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Itenium.SkillForge.Data;

public static class SeedData
{
    public static async Task SeedDevelopmentData(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await SeedTeams(db);
        await SeedCourses(db);
        await app.SeedTestUsers();
        await app.SeedConsultants(db);
    }

    private static async Task SeedTeams(AppDbContext db)
    {
        if (!await db.Teams.AnyAsync())
        {
            db.Teams.AddRange(
                new TeamEntity { Id = 1, Name = "Java" },
                new TeamEntity { Id = 2, Name = ".NET" },
                new TeamEntity { Id = 3, Name = "PO & Analysis" },
                new TeamEntity { Id = 4, Name = "QA" });
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedCourses(AppDbContext db)
    {
        if (!await db.Courses.AnyAsync())
        {
            db.Courses.AddRange(
                new CourseEntity { Id = 1, Name = "Introduction to Programming", Description = "Learn the basics of programming", Category = "Development", Level = "Beginner" },
                new CourseEntity { Id = 2, Name = "Advanced C#", Description = "Master C# programming language", Category = "Development", Level = "Advanced" },
                new CourseEntity { Id = 3, Name = "Cloud Architecture", Description = "Design scalable cloud solutions", Category = "Architecture", Level = "Intermediate" },
                new CourseEntity { Id = 4, Name = "Agile Project Management", Description = "Learn agile methodologies", Category = "Management", Level = "Beginner" });
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedTestUsers(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ForgeUser>>();

        // BackOffice admin - no team claim (manages all)
        if (await userManager.FindByEmailAsync("backoffice@test.local") == null)
        {
            var admin = new ForgeUser
            {
                UserName = "backoffice",
                Email = "backoffice@test.local",
                EmailConfirmed = true,
                FirstName = "BackOffice",
                LastName = "Admin"
            };
            var result = await userManager.CreateAsync(admin, "AdminPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRolesAsync(admin, ["backoffice"]);
            }
        }

        // Local user for Java team only
        if (await userManager.FindByEmailAsync("java@test.local") == null)
        {
            var user = new ForgeUser
            {
                UserName = "java",
                Email = "java@test.local",
                EmailConfirmed = true,
                FirstName = "Java",
                LastName = "Developer"
            };
            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "manager");
                await userManager.AddClaimAsync(user, new Claim("team", "1")); // Java
            }
        }

        // Local user for .NET team only
        if (await userManager.FindByEmailAsync("dotnet@test.local") == null)
        {
            var user = new ForgeUser
            {
                UserName = "dotnet",
                Email = "dotnet@test.local",
                EmailConfirmed = true,
                FirstName = "DotNet",
                LastName = "Developer"
            };
            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "manager");
                await userManager.AddClaimAsync(user, new Claim("team", "2")); // .NET
            }
        }

        // User with access to multiple teams (Java + .NET)
        if (await userManager.FindByEmailAsync("multi@test.local") == null)
        {
            var user = new ForgeUser
            {
                UserName = "multi",
                Email = "multi@test.local",
                EmailConfirmed = true,
                FirstName = "Multi",
                LastName = "Team"
            };
            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "manager");
                await userManager.AddClaimAsync(user, new Claim("team", "1")); // Java
                await userManager.AddClaimAsync(user, new Claim("team", "2")); // .NET
            }
        }

        // Learner user - basic learner role
        if (await userManager.FindByEmailAsync("learner@test.local") == null)
        {
            var user = new ForgeUser
            {
                UserName = "learner",
                Email = "learner@test.local",
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = "Learner"
            };
            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "learner");
            }
        }
    }

    private static async Task SeedConsultants(this WebApplication app, AppDbContext db)
    {
        if (await db.ConsultantProfiles.AnyAsync())
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ForgeUser>>();

        // Seed data: (email, firstName, lastName, teamId, daysAgoActivity)
        // null daysAgo = never active (shown as inactive)
        (string Email, string First, string Last, int TeamId, int? DaysAgo)[] consultants =
        [
            // .NET team — coach: dotnet@test.local (team 2)
            ("lea@test.local",    "Lea",    "Van Den Berg", 2, 2),    // active
            ("thomas@test.local", "Thomas", "De Smedt",     2, 25),   // inactive — no activity 25 days
            ("amber@test.local",  "Amber",  "Jacobs",       2, 8),    // active
            ("olivier@test.local","Olivier","Maes",          2, null), // inactive — never active

            // Java team — coach: java@test.local (team 1)
            ("sander@test.local", "Sander", "Claes",        1, 1),    // active
            ("lucas@test.local",  "Lucas",  "Peeters",      1, 30),   // inactive
            ("emma@test.local",   "Emma",   "Willems",      1, 5),    // active

            // QA team (team 4)
            ("sophie@test.local", "Sophie", "Goossens",     4, 10),   // active
            ("noah@test.local",   "Noah",   "Vermeersch",   4, 28),   // inactive

            // PO & Analysis team (team 3)
            ("julie@test.local",  "Julie",  "Dubois",       3, 3),    // active
            ("max@test.local",    "Max",    "Leemans",      3, null),  // inactive — never active
        ];

        foreach (var (email, first, last, teamId, daysAgo) in consultants)
        {
            if (await userManager.FindByEmailAsync(email) != null)
            {
                continue;
            }

            var user = new ForgeUser
            {
                UserName = email.Split('@')[0],
                Email = email,
                EmailConfirmed = true,
                FirstName = first,
                LastName = last,
            };

            var result = await userManager.CreateAsync(user, "UserPassword123!");
            if (!result.Succeeded)
            {
                continue;
            }

            await userManager.AddToRoleAsync(user, "learner");

            db.ConsultantProfiles.Add(new ConsultantProfileEntity
            {
                UserId = user.Id,
                TeamId = teamId,
                LastActivityAt = daysAgo.HasValue
                    ? DateTime.UtcNow.AddDays(-daysAgo.Value)
                    : null,
            });
        }

        await db.SaveChangesAsync();
    }
}
