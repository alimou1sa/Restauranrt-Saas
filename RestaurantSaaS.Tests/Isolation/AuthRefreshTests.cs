using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Domain.Common;
using RestaurantSaaS.Application.DTOs.RefreshTokens.RefreshTokensRequest;
using RestaurantSaaS.Tests.TestSupport;
using Xunit;

namespace RestaurantSaaS.Tests.Isolation;

/// <summary>
/// Refresh-token rotation with reuse detection: each refresh issues a new token and revokes the
/// old one; replaying a revoked token is treated as theft and kills every session for the
/// membership.
/// </summary>
public sealed class AuthRefreshTests
{
    [Fact]
    public async Task Refresh_rotates_the_token_and_revokes_the_previous_one()
    {
        await using var f = await AuthFixture.CreateAsync();
        var (c, _, first) = await AuthScenario.SelectOrganizationAAsync(f);
        await using (c)
        {
            var second = await c.Auth.RefreshAsync(new RefreshTokenRequest { RefreshToken = first });

            Assert.NotEqual(first, second.RefreshToken);
            Assert.Equal(TokenTypes.Access, Jwt.Claim(second.Token, AppClaimTypes.TokenType));
            Assert.Equal(f.World.OrgA.OrganizationId.ToString(), Jwt.Claim(second.Token, AppClaimTypes.OrganizationId));

            var tokens = await c.Context.RefreshTokens.ToListAsync();
            Assert.Equal(2, tokens.Count);

            var old = tokens.Single(t => t.TokenHash == f.Harness.RefreshHasher.Hash(first));
            var fresh = tokens.Single(t => t.TokenHash == f.Harness.RefreshHasher.Hash(second.RefreshToken));

            Assert.NotNull(old.RevokedAtUtc);
            Assert.Null(fresh.RevokedAtUtc);
            Assert.Equal(fresh.RefreshTokenId, old.ReplacedByTokenId);
        }
    }

    [Fact]
    public async Task Replaying_a_revoked_token_revokes_every_session_for_the_membership()
    {
        await using var f = await AuthFixture.CreateAsync();
        var (c, _, first) = await AuthScenario.SelectOrganizationAAsync(f);
        string replacement;
        await using (c)
        {
            var second = await c.Auth.RefreshAsync(new RefreshTokenRequest { RefreshToken = first });
            replacement = second.RefreshToken;

            // Replay of the already-rotated token must fail.
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => c.Auth.RefreshAsync(new RefreshTokenRequest { RefreshToken = first }));
        }

        // Verify through a fresh context: revoke-all uses ExecuteUpdate, which bypasses the change tracker.
        await using (var verify = f.Db.CreateContext())
        {
            var tokens = await verify.RefreshTokens.ToListAsync();
            Assert.Contains(tokens, t => t.TokenHash == f.Harness.RefreshHasher.Hash(replacement));
            Assert.All(tokens, t => Assert.NotNull(t.RevokedAtUtc));
        }

        // The replacement token is dead too.
        var user = new FakeCurrentUser { UserId = f.World.OrgA.UserId };
        await using var fresh = f.Harness.Create(f.World.OrgA.OrganizationId, user);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => fresh.Auth.RefreshAsync(new RefreshTokenRequest { RefreshToken = replacement }));
    }

    [Fact]
    public async Task Refresh_with_an_unknown_token_is_rejected()
    {
        await using var f = await AuthFixture.CreateAsync();
        var (c, _, _) = await AuthScenario.SelectOrganizationAAsync(f);
        await using (c)
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => c.Auth.RefreshAsync(new RefreshTokenRequest { RefreshToken = "not-a-real-refresh-token" }));
        }
    }

    [Fact]
    public async Task Refresh_with_an_expired_token_is_rejected()
    {
        await using var f = await AuthFixture.CreateAsync();
        var (setup, _, first) = await AuthScenario.SelectOrganizationAAsync(f);
        await setup.DisposeAsync();

        await using (var admin = f.Db.CreateContext())
        {
            var stored = await admin.RefreshTokens.SingleAsync();
            stored.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
            await admin.SaveChangesAsync();
        }

        // A fresh context represents the next request; the original one still tracks the old expiry.
        var user = new FakeCurrentUser { UserId = f.World.OrgA.UserId };
        await using var c = f.Harness.Create(f.World.OrgA.OrganizationId, user);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => c.Auth.RefreshAsync(new RefreshTokenRequest { RefreshToken = first }));
    }
}
