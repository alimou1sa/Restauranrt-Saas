using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using RestaurantSaaS.Application.Services;
using RestaurantSaaS.Infrastructure.Auth;
using RestaurantSaaS.Tests.TestSupport;
using System.Security.Claims;
using Xunit;

namespace RestaurantSaaS.Tests.Isolation;

/// <summary>
/// RBAC resolution and the authorization handler: permissions come from active roles of an active
/// membership, and never cross an organization boundary.
/// </summary>
public sealed class PermissionIsolationTests
{
    private static async Task<HashSet<string>> ResolveAsync(PermissionFixture f, int organizationId, int membershipId)
    {
        var tenant = IsolationDatabase.NewTenant(organizationId, membershipId);
        await using var ctx = f.Db.CreateContext(tenant);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        return await new PermissionService(ctx, cache).GetPermissionsAsync(membershipId);
    }

    private static async Task<bool> HandlerAllowsAsync(PermissionFixture f, FakeCurrentUser user, int? organizationId, int? organizationUserId, string code)
    {
        var tenant = IsolationDatabase.NewTenant(organizationId, organizationUserId);
        await using var ctx = f.Db.CreateContext(tenant);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var permissions = new PermissionService(ctx, cache);
        var handler = new PermissionAuthorizationHandler(user, tenant, permissions);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "tester") }, "test"));
        var context = new AuthorizationHandlerContext(new IAuthorizationRequirement[] { new PermissionRequirement(code) }, principal, resource: null);
        await handler.HandleAsync(context);
        return context.HasSucceeded;
    }

    [Fact]
    public async Task A_system_role_grants_its_permissions()
    {
        await using var f = await PermissionFixture.CreateAsync();

        var permissions = await ResolveAsync(f, f.World.OrgAId, f.World.SystemUserMembershipId);

        Assert.Equal(new[] { "order.read" }, permissions.ToArray());
    }

    [Fact]
    public async Task A_custom_role_grants_its_permissions()
    {
        await using var f = await PermissionFixture.CreateAsync();

        var permissions = await ResolveAsync(f, f.World.OrgAId, f.World.CustomUserMembershipId);

        Assert.Equal(new[] { "order.manage" }, permissions.ToArray());
    }

    [Fact]
    public async Task A_membership_without_roles_gets_nothing()
    {
        await using var f = await PermissionFixture.CreateAsync();

        Assert.Empty(await ResolveAsync(f, f.World.OrgAId, f.World.NoRoleMembershipId));
    }

    [Fact]
    public async Task An_inactive_membership_gets_nothing_even_though_it_has_a_role()
    {
        await using var f = await PermissionFixture.CreateAsync();

        Assert.Empty(await ResolveAsync(f, f.World.OrgAId, f.World.InactiveMembershipId));
    }

    [Fact]
    public async Task A_removed_membership_gets_nothing_even_though_it_has_a_role()
    {
        await using var f = await PermissionFixture.CreateAsync();

        Assert.Empty(await ResolveAsync(f, f.World.OrgAId, f.World.RemovedMembershipId));
    }

    [Fact]
    public async Task A_membership_whose_role_is_inactive_gets_nothing()
    {
        await using var f = await PermissionFixture.CreateAsync();

        Assert.Empty(await ResolveAsync(f, f.World.OrgAId, f.World.InactiveRoleMembershipId));
    }

    [Fact]
    public async Task Permissions_do_not_leak_to_another_organization()
    {
        await using var f = await PermissionFixture.CreateAsync();

        // Organization B context asking for organization A's membership.
        Assert.Empty(await ResolveAsync(f, f.World.OrgBId, f.World.SystemUserMembershipId));
    }

    [Fact]
    public async Task Handler_grants_when_the_membership_has_the_permission()
    {
        await using var f = await PermissionFixture.CreateAsync();
        var user = new FakeCurrentUser { UserId = 1 };

        Assert.True(await HandlerAllowsAsync(f, user, f.World.OrgAId, f.World.SystemUserMembershipId, "order.read"));
    }

    [Fact]
    public async Task Handler_denies_when_the_membership_lacks_the_permission()
    {
        await using var f = await PermissionFixture.CreateAsync();
        var user = new FakeCurrentUser { UserId = 1 };

        Assert.False(await HandlerAllowsAsync(f, user, f.World.OrgAId, f.World.SystemUserMembershipId, "order.manage"));
        Assert.False(await HandlerAllowsAsync(f, user, f.World.OrgAId, f.World.NoRoleMembershipId, "order.read"));
    }

    [Fact]
    public async Task Handler_denies_without_an_organization_user_context()
    {
        await using var f = await PermissionFixture.CreateAsync();
        var user = new FakeCurrentUser { UserId = 1 };

        Assert.False(await HandlerAllowsAsync(f, user, organizationId: null, organizationUserId: null, "order.read"));
        Assert.False(await HandlerAllowsAsync(f, user, f.World.OrgAId, organizationUserId: null, "order.read"));
    }

    [Fact]
    public async Task Handler_grants_a_platform_admin_with_an_organization_context()
    {
        await using var f = await PermissionFixture.CreateAsync();
        var admin = new FakeCurrentUser { UserId = 1, IsPlatformAdmin = true };

        // Documented behaviour: a platform admin acting inside an organization passes every permission check.
        Assert.True(await HandlerAllowsAsync(f, admin, f.World.OrgAId, f.World.NoRoleMembershipId, "anything.at.all"));
    }

    [Fact]
    public async Task Handler_denies_a_platform_admin_without_an_organization_context()
    {
        await using var f = await PermissionFixture.CreateAsync();
        var admin = new FakeCurrentUser { UserId = 1, IsPlatformAdmin = true };

        Assert.False(await HandlerAllowsAsync(f, admin, organizationId: null, organizationUserId: null, "order.read"));
    }
}
