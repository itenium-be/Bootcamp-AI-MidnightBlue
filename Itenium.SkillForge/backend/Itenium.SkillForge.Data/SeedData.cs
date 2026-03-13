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
        await SeedResources(db);
        await SeedGoals(db);
        await app.SeedTestUsers();
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

    private static async Task SeedResources(AppDbContext db)
    {
        if (!await db.Resources.AnyAsync())
        {
            db.Resources.AddRange(
                new ResourceEntity { Id = 1, Title = "C# in Depth", Url = "https://csharpindepth.com", Type = ResourceType.Book, SkillId = 2, FromLevel = 2, ToLevel = 4, Description = "Comprehensive guide to C# programming" },
                new ResourceEntity { Id = 2, Title = "Introduction to C#", Url = "https://learn.microsoft.com/dotnet/csharp", Type = ResourceType.Article, SkillId = 1, FromLevel = 1, ToLevel = 2, Description = "Official Microsoft C# introduction" },
                new ResourceEntity { Id = 3, Title = "Cloud Architecture Patterns", Url = "https://example.com/cloud-patterns", Type = ResourceType.Video, SkillId = 3, FromLevel = 2, ToLevel = 5, Description = "Video series on cloud architecture patterns" },
                new ResourceEntity { Id = 4, Title = "Agile & Scrum Fundamentals", Url = "https://example.com/agile-course", Type = ResourceType.Course, SkillId = 4, FromLevel = 1, ToLevel = 3, Description = "Online course covering agile and scrum basics" });
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedGoals(AppDbContext db)
    {
        if (!await db.Goals.AnyAsync())
        {
            db.Goals.AddRange(
                new GoalEntity { Id = 1, ConsultantId = "learner", Title = "Clean Code niveau 3", SkillId = 1, TargetLevel = 3, Deadline = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), Notes = "Focus on naming, functions, and code structure.", CreatedBy = "dotnet" },
                new GoalEntity { Id = 2, ConsultantId = "learner", Title = "Entity Framework niveau 2", SkillId = 2, TargetLevel = 2, Deadline = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), Notes = "Master EF Core querying and migrations.", CreatedBy = "dotnet" },
                new GoalEntity { Id = 3, ConsultantId = "learner", Title = "REST API Design niveau 2", SkillId = 3, TargetLevel = 2, Deadline = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc), Notes = "Apply REST principles in current mission.", CreatedBy = "dotnet" });
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
}
