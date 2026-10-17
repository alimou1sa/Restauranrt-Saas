using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Domain.Common;
using RestaurantSaaS.Application.DTOs.Auth.Request;
using RestaurantSaaS.Application.DTOs.RefreshTokens.RefreshTokensRequest;
using RestaurantSaaS.Tests.TestSupport;
using Xunit;

namespace RestaurantSaaS.Tests.Isolation;

/// <summary>Logout revocation, the /me profile context check, and platform-admin impersonation.</summary>
public sealed class AuthSessionAndProfileTests
{
    [Fact]
    public async Task Logout_revokes_the_refresh_token_and_blacklists_the_access_token()
    {
        await using var f = await AuthFixture.CreateAsync();
        var user = new FakeCurrentUser { UserId = f.World.OrgA.UserId, Jti = "test-jti", ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10) };
        await using var c = f.Harness.Create(f.World.OrgA.OrganizationId, user);

        var tokens = await c.Auth.SelectOrganizationAsync(new SelectOrganizationRequest { OrganizationId = f.World.OrgA.OrganizationId });
        await c.Auth.LogoutAsync(new LogoutRequest { RefreshToken = tokens.RefreshToken });

        Assert.NotNull((await c.Context.RefreshTokens.SingleAsync()).RevokedAtUtc);
        Assert.True(c.Blacklist.IsRevoked("test-jti"));
    }

    [Fact]
    public async Task Logout_rejects_a_refresh_token_that_belongs_to_another_user()
    {
        await using var f = await AuthFixture.CreateAsync();

        var userB = new FakeCurrentUser { UserId = f.World.OrgB.UserId };
        string refreshOfB;
        await using (var cb = f.Harness.Create(f.World.OrgB.OrganizationId, userB))
        {
            refreshOfB = (await cb.Auth.SelectOrganizationAsync(
                new SelectOrganizationRequest { OrganizationId = f.World.OrgB.OrganizationId })).RefreshToken;
        }

        var userA = new FakeCurrentUser { UserId = f.World.OrgA.UserId };
        await using var ca = f.Harness.Create(f.World.OrgA.OrganizationId, userA);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => ca.Auth.LogoutAsync(new LogoutRequest { RefreshToken = refreshOfB }));
    }

    [Fact]
    public async Task GetMe_returns_the_membership_matching_the_current_tenant_context()
    {
        await using var f = await AuthFixture.CreateAsync();
        var user = new FakeCurrentUser { UserId = f.World.OrgA.UserId };
        await using var c = f.Harness.Create(f.World.OrgA.OrganizationId, user, organizationUserId: f.World.OrgA.OrganizationUserId);

        var me = await c.Auth.GetMeAsync();

        Assert.False(me.IsPlatformAdmin);
        Assert.Equal(f.World.OrgA.OrganizationId, me.OrganizationId);
        Assert.Equal(f.World.OrgA.OrganizationUserId, me.OrganizationUserId);
        Assert.Equal(f.World.OrgA.Branch("A1").BranchId, me.BranchId);
        Assert.Equal(f.EmailOfA, me.Email);
        Assert.Equal("Manager", Assert.Single(me.Roles));
        Assert.Empty(me.Permissions);
    }

    [Fact]
    public async Task GetMe_rejects_a_membership_that_is_not_the_current_context()
    {
        await using var f = await AuthFixture.CreateAsync();
        var user = new FakeCurrentUser { UserId = f.World.OrgA.UserId };
        // Organization A context but the membership id of organization B's user.
        await using var c = f.Harness.Create(f.World.OrgA.OrganizationId, user, organizationUserId: f.World.OrgB.OrganizationUserId);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => c.Auth.GetMeAsync());
    }

    [Fact]
    public async Task GetMe_for_platform_admin_returns_the_platform_flag_and_no_permissions()
    {
        await using var f = await AuthFixture.CreateAsync();
        var user = new FakeCurrentUser { UserId = f.World.OrgA.UserId, IsPlatformAdmin = true };
        await using var c = f.Harness.Create(null, user);

        var me = await c.Auth.GetMeAsync();

        Assert.True(me.IsPlatformAdmin);
        Assert.Equal(f.World.OrgA.UserId, me.UserId);
        Assert.Empty(me.Permissions);
        Assert.Empty(me.Roles);
    }

    [Fact]
    public async Task Impersonate_requires_a_platform_admin()
    {
        await using var f = await AuthFixture.CreateAsync();
        var user = new FakeCurrentUser { UserId = f.World.OrgA.UserId, IsPlatformAdmin = false };
        await using var c = f.Harness.Create(null, user);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => c.Auth.ImpersonateOrganizationAsync(f.World.OrgA.OrganizationId));
    }

    [Fact]
    public async Task Impersonate_issues_an_access_token_scoped_to_the_organization()
    {
        await using var f = await AuthFixture.CreateAsync();
        var user = new FakeCurrentUser { UserId = f.World.OrgA.UserId, IsPlatformAdmin = true };
        await using var c = f.Harness.Create(null, user);

        var res = await c.Auth.ImpersonateOrganizationAsync(f.World.OrgA.OrganizationId);

        Assert.Equal(f.World.OrgA.OrganizationId, res.OrganizationId);
        Assert.Equal(TokenTypes.Access, Jwt.Claim(res.Token, AppClaimTypes.TokenType));
        Assert.Equal(f.World.OrgA.OrganizationId.ToString(), Jwt.Claim(res.Token, AppClaimTypes.OrganizationId));
        // Documented behaviour: an impersonation token is both an access token and a platform-admin token.
        Assert.Equal("true", Jwt.Claim(res.Token, AppClaimTypes.IsPlatformAdmin));
    }
}
