using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs.Auth.Request;

namespace RestaurantSaaS.Tests.TestSupport;

/// <summary>Small shared steps used by the auth tests.</summary>
public static class AuthScenario
{
    /// <summary>Runs the two-step sign-in for organization A and returns the tokens plus a live service.</summary>
    public static async Task<(AuthComponents Components, string AccessToken, string RefreshToken)> SelectOrganizationAAsync(AuthFixture fixture)
    {
        var user = new FakeCurrentUser { UserId = fixture.World.OrgA.UserId };
        var components = fixture.Harness.Create(fixture.World.OrgA.OrganizationId, user);
        var tokens = await components.Auth.SelectOrganizationAsync(
            new SelectOrganizationRequest { OrganizationId = fixture.World.OrgA.OrganizationId });

        return (components, tokens.Token, tokens.RefreshToken);
    }

    /// <summary>Deactivates an organization outside any service under test.</summary>
    public static async Task SetOrganizationActiveAsync(AuthFixture fixture, int organizationId, bool isActive)
    {
        await using var ctx = fixture.Db.CreateContext();
        var organization = await ctx.Organizations.SingleAsync(o => o.OrganizationId == organizationId);
        organization.IsActive = isActive;
        await ctx.SaveChangesAsync();
    }

    /// <summary>Activates or deactivates a user outside any service under test.</summary>
    public static async Task SetUserActiveAsync(AuthFixture fixture, int userId, bool isActive)
    {
        await using var ctx = fixture.Db.CreateContext();
        var user = await ctx.Users.SingleAsync(u => u.UserId == userId);
        user.IsActive = isActive;
        await ctx.SaveChangesAsync();
    }

    /// <summary>Marks a user as a platform administrator.</summary>
    public static async Task SetPlatformAdminAsync(AuthFixture fixture, int userId, bool isPlatformAdmin)
    {
        await using var ctx = fixture.Db.CreateContext();
        var user = await ctx.Users.SingleAsync(u => u.UserId == userId);
        user.IsPlatformAdmin = isPlatformAdmin;
        await ctx.SaveChangesAsync();
    }
}
