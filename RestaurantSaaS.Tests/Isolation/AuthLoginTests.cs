using RestaurantSaaS.Domain.Common;
using RestaurantSaaS.Application.DTOs.Auth.Request;
using RestaurantSaaS.Tests.TestSupport;
using Xunit;

namespace RestaurantSaaS.Tests.Isolation;

/// <summary>
/// The first sign-in step must only ever hand out a short-lived organization-selection token,
/// and only to an active user of an active organization.
/// </summary>
public sealed class AuthLoginTests
{
    [Fact]
    public async Task Login_returns_a_preauth_token_listing_only_the_users_organizations()
    {
        await using var f = await AuthFixture.CreateAsync();
        await using var c = f.Harness.Create();

        var res = await c.Auth.LoginAsync(new LoginRequest { Email = f.EmailOfA, Password = AuthFixture.Password });

        Assert.Equal(TokenTypes.PreAuth, res.TokenType);
        Assert.Equal(TokenTypes.PreAuth, Jwt.Claim(res.Token, AppClaimTypes.TokenType));

        var organization = Assert.Single(res.Organizations);
        Assert.Equal(f.World.OrgA.OrganizationId, organization.OrganizationId);

        // A pre-auth token must not carry any organization context yet.
        Assert.False(Jwt.HasClaim(res.Token, AppClaimTypes.OrganizationId));
        Assert.False(Jwt.HasClaim(res.Token, AppClaimTypes.OrganizationUserId));
    }

    [Fact]
    public async Task Login_with_a_wrong_password_is_rejected()
    {
        await using var f = await AuthFixture.CreateAsync();
        await using var c = f.Harness.Create();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => c.Auth.LoginAsync(new LoginRequest { Email = f.EmailOfA, Password = "not-the-password" }));
    }

    [Fact]
    public async Task Login_for_an_inactive_user_is_rejected()
    {
        await using var f = await AuthFixture.CreateAsync();
        await AuthScenario.SetUserActiveAsync(f, f.World.OrgA.UserId, isActive: false);
        await using var c = f.Harness.Create();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => c.Auth.LoginAsync(new LoginRequest { Email = f.EmailOfA, Password = AuthFixture.Password }));
    }

    [Fact]
    public async Task Login_for_a_user_in_no_active_organization_is_rejected()
    {
        await using var f = await AuthFixture.CreateAsync();
        await AuthScenario.SetOrganizationActiveAsync(f, f.World.OrgA.OrganizationId, isActive: false);
        await using var c = f.Harness.Create();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => c.Auth.LoginAsync(new LoginRequest { Email = f.EmailOfA, Password = AuthFixture.Password }));
    }

    [Fact]
    public async Task Login_as_platform_admin_returns_a_platform_token_without_organizations()
    {
        await using var f = await AuthFixture.CreateAsync();
        await AuthScenario.SetPlatformAdminAsync(f, f.World.OrgA.UserId, isPlatformAdmin: true);
        await using var c = f.Harness.Create();

        var res = await c.Auth.LoginAsync(new LoginRequest { Email = f.EmailOfA, Password = AuthFixture.Password });

        Assert.Equal(TokenTypes.PlatformAdmin, res.TokenType);
        Assert.Equal(TokenTypes.PlatformAdmin, Jwt.Claim(res.Token, AppClaimTypes.TokenType));
        Assert.Equal("true", Jwt.Claim(res.Token, AppClaimTypes.IsPlatformAdmin));
        Assert.Empty(res.Organizations);
        Assert.False(Jwt.HasClaim(res.Token, AppClaimTypes.OrganizationId));
    }
}
