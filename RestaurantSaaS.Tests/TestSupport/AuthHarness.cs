using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RestaurantSaaS.Application.InterfacesService;
using RestaurantSaaS.Application.Services;
using RestaurantSaaS.Infrastructure.Auth;
using RestaurantSaaS.Infrastructure.Data;
using RestaurantSaaS.Infrastructure.PasswordHashe;

namespace RestaurantSaaS.Tests.TestSupport;

/// <summary>An <see cref="AuthService"/> wired with the real collaborators plus the two fakes.</summary>
public sealed class AuthComponents : IAsyncDisposable
{
    public required AuthService Auth { get; init; }
    public required AppDbContext Context { get; init; }
    public required FakeCurrentTenant Tenant { get; init; }
    public required IAccessTokenBlacklist Blacklist { get; init; }
    public required PermissionService Permissions { get; init; }
    public required IMemoryCache Cache { get; init; }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        Cache.Dispose();
    }
}

/// <summary>
/// Builds real <c>AuthService</c>/<c>PermissionService</c> instances against the throwaway database.
/// Only the request-scoped identity (<see cref="FakeCurrentUser"/>) and tenant (<see cref="FakeCurrentTenant"/>)
/// are faked - everything else (hashing, JWT signing, refresh-token hashing, permission resolution) is real.
/// </summary>
public sealed class AuthHarness
{
    private readonly IsolationDatabase _db;

    public AuthHarness(IsolationDatabase db)
    {
        _db = db;
        Settings = new JwtSettings
        {
            Key = "isolation-tests-signing-key-that-is-long-enough-for-hmac-sha256!!",
            Issuer = "RestaurantSaaS.Tests",
            Audience = "RestaurantSaaS.Tests.Clients",
            ExpirationMinutes = 15,
            RefreshTokenExpirationDays = 30
        };
    }

    public JwtSettings Settings { get; }

    public IRefreshTokenHasher RefreshHasher { get; } = new RefreshTokenHasher();

    public async Task SetPasswordAsync(int userId, string password)
    {
        await using var ctx = _db.CreateContext();
        var user = await ctx.Users.SingleAsync(u => u.UserId == userId);
        user.PasswordHash = new PasswordHasher().Hash(password);
        await ctx.SaveChangesAsync();
    }

    public AuthComponents Create(
        int? organizationId = null,
        FakeCurrentUser? user = null,
        int? organizationUserId = null,
        int? branchId = null)
    {
        var tenant = IsolationDatabase.NewTenant(organizationId, organizationUserId, branchId);
        var context = new AppDbContext(_db.BuildOptions(), tenant);
        var cache = new MemoryCache(new MemoryCacheOptions());
        var blacklist = new AccessTokenBlacklist(cache);
        var permissions = new PermissionService(context, cache);
        var auth = new AuthService(
            context,
            new PasswordHasher(),
            new JwtTokenGenerator(Options.Create(Settings)),
            user ?? new FakeCurrentUser { UserId = 0 },
            Settings,
            RefreshHasher,
            blacklist,
            tenant,
            permissions);

        return new AuthComponents
        {
            Auth = auth,
            Context = context,
            Tenant = tenant,
            Blacklist = blacklist,
            Permissions = permissions,
            Cache = cache
        };
    }
}
