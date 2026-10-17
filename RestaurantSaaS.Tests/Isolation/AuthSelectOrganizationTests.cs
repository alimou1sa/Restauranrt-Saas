using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Domain.Common;
using RestaurantSaaS.Application.DTOs.Auth.Request;
using RestaurantSaaS.Application.DTOs.RefreshTokens.RefreshTokensRequest;
using RestaurantSaaS.Tests.TestSupport;
using Xunit;

namespace RestaurantSaaS.Tests.Isolation;

/// <summary>
/// Organization selection issues the real access + refresh tokens; a user can only select a
/// membership they actually hold, in an active organization.
/// </summary>
public sealed class AuthSelectOrganizationTests
{
    [Fact]
    public async Task Select_issues_an_access_token_bound_to_the_organization_and_a_hashed_refresh_token()
    {
        await using var f = await AuthFixture.CreateAsync();
        var user = new FakeCurrentUser { UserId = f.World.OrgA.UserId };
        await using var c = f.Harness.Create(f.World.OrgA.OrganizationId, user);

        var res = await c.Auth.SelectOrganizationAsync(new SelectOrganizationRequest { OrganizationId = f.World.OrgA.OrganizationId });

        Assert.Equal(TokenTypes.Access, Jwt.Claim(res.Token, AppClaimTypes.TokenType));
        Assert.Equal(f.World.OrgA.OrganizationId.ToString(), Jwt.Claim(res.Token, AppClaimTypes.OrganizationId));
        Assert.Equal(f.World.OrgA.OrganizationUserId.ToString(), Jwt.Claim(res.Token, AppClaimTypes.OrganizationUserId));
        Assert.Equal(f.World.OrgA.OrganizationId, res.OrganizationId);
        Assert.Equal(f.World.OrgA.OrganizationUserId, res.OrganizationUserId);
        Assert.False(string.IsNullOrWhiteSpace(res.RefreshToken));

        var stored = await c.Context.RefreshTokens.SingleAsync();
        Assert.Equal(f.Harness.RefreshHasher.Hash(res.RefreshToken), stored.TokenHash);
        Assert.NotEqual(res.RefreshToken, stored.TokenHash);
        Assert.Equal(f.World.OrgA.OrganizationUserId, stored.OrganizationUserId);
    }

    [Fact]
    public async Task Select_rejects_a_membership_the_user_does_not_hold()
    {
        await using var f = await AuthFixture.CreateAsync();
        var userOfA = new FakeCurrentUser { UserId = f.World.OrgA.UserId };
        // No tenant context on purpose: selection must rely on the user claim, not on filters.
        await using var c = f.Harness.Create(null, userOfA);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => c.Auth.SelectOrganizationAsync(new SelectOrganizationRequest { OrganizationId = f.World.OrgB.OrganizationId }));
    }

    [Fact]
    public async Task Select_rejects_an_inactive_organization()
    {
        await using var f = await AuthFixture.CreateAsync();
        await AuthScenario.SetOrganizationActiveAsync(f, f.World.OrgA.OrganizationId, isActive: false);
        var user = new FakeCurrentUser { UserId = f.World.OrgA.UserId };
        await using var c = f.Harness.Create(null, user);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => c.Auth.SelectOrganizationAsync(new SelectOrganizationRequest { OrganizationId = f.World.OrgA.OrganizationId }));
    }

    [Fact]
    public async Task Refresh_tokens_are_never_persisted_in_plain_text()
    {
        await using var f = await AuthFixture.CreateAsync();
        var (c, _, refresh) = await AuthScenario.SelectOrganizationAAsync(f);
        await using (c)
        {
            var hashes = await c.Context.RefreshTokens.Select(t => t.TokenHash).ToListAsync();
            Assert.DoesNotContain(refresh, hashes);
            Assert.Contains(f.Harness.RefreshHasher.Hash(refresh), hashes);
        }
    }
}
