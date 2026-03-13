using Itenium.Forge.Security.OpenIddict;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Itenium.SkillForge.WebApi.Tests;

public abstract class IdentityTestBase : DatabaseTestBase
{
    protected UserManager<ForgeUser> UserManager { get; private set; } = null!;
    protected RoleManager<IdentityRole> RoleManager { get; private set; } = null!;

    [SetUp]
    public async Task IdentitySetUp()
    {
        var userStore = new UserStore<ForgeUser, IdentityRole, Data.AppDbContext>(Db);
        var roleStore = new RoleStore<IdentityRole, Data.AppDbContext>(Db);

        RoleManager = new RoleManager<IdentityRole>(
            roleStore,
            [new RoleValidator<IdentityRole>()],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            NullLogger<RoleManager<IdentityRole>>.Instance
        );

        UserManager = new UserManager<ForgeUser>(
            userStore,
            Options.Create(new IdentityOptions()),
            new PasswordHasher<ForgeUser>(),
            [new UserValidator<ForgeUser>()],
            [new PasswordValidator<ForgeUser>()],
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            Substitute.For<IServiceProvider>(),
            NullLogger<UserManager<ForgeUser>>.Instance
        );

        foreach (var role in new[] { "backoffice", "manager", "learner" })
        {
            if (!await RoleManager.RoleExistsAsync(role))
            {
                await RoleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    [TearDown]
    public void IdentityTearDown()
    {
        UserManager.Dispose();
        RoleManager.Dispose();
    }
}
