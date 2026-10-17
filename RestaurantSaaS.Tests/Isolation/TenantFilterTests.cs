using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Tests.TestSupport;
using Xunit;

namespace RestaurantSaaS.Tests.Isolation;

/// <summary>
/// Proves that the global query filters in <c>AppDbContext</c> really isolate organizations:
/// no context sees another organization's rows, and a context with no organization at all sees nothing.
/// </summary>
public sealed class TenantFilterTests : IAsyncLifetime
{
    private readonly IsolationDatabase _db = new();
    private SeededWorld _world = null!;

    public async Task InitializeAsync()
    {
        await _db.InitializeAsync();
        _world = await TestData.SeedAsync(_db);
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    [Fact]
    public async Task No_organization_context_sees_no_tenant_rows()
    {
        await using var ctx = _db.CreateContext();

        Assert.Equal(0, await ctx.Branches.CountAsync());
        Assert.Equal(0, await ctx.Customers.CountAsync());
        Assert.Equal(0, await ctx.Roles.CountAsync());
        Assert.Equal(0, await ctx.Products.CountAsync());
        Assert.Equal(0, await ctx.Orders.CountAsync());
        Assert.Equal(0, await ctx.Menus.CountAsync());
        Assert.Equal(0, await ctx.Categories.CountAsync());
        Assert.Equal(0, await ctx.RestaurantTables.CountAsync());
        Assert.Equal(0, await ctx.Inventories.CountAsync());
        Assert.Equal(0, await ctx.InventoryTransactions.CountAsync());
        Assert.Equal(0, await ctx.OrderItems.CountAsync());
        Assert.Equal(0, await ctx.Payments.CountAsync());
        Assert.Equal(0, await ctx.OrganizationUsers.CountAsync());
        Assert.Equal(0, await ctx.Subscriptions.CountAsync());
        Assert.Equal(0, await ctx.UserRoles.CountAsync());
    }

    [Fact]
    public async Task The_seed_data_really_exists_when_filters_are_ignored()
    {
        await using var ctx = _db.CreateContext();

        Assert.Equal(3, await ctx.Branches.IgnoreQueryFilters().CountAsync());
        Assert.Equal(3, await ctx.Orders.IgnoreQueryFilters().CountAsync());
        Assert.Equal(3, await ctx.Menus.IgnoreQueryFilters().CountAsync());
    }

    [Fact]
    public async Task Organization_A_sees_only_its_own_rows()
    {
        await using var ctx = _db.CreateContext(_world.OrgA.OrganizationId);

        Assert.Equal(2, await ctx.Branches.CountAsync());
        Assert.Equal(1, await ctx.Customers.CountAsync());
        Assert.Equal(1, await ctx.Roles.CountAsync());
        Assert.Equal(1, await ctx.OrganizationUsers.CountAsync());
        Assert.Equal(1, await ctx.Subscriptions.CountAsync());
        Assert.Equal(1, await ctx.UserRoles.CountAsync());
        Assert.Equal(2, await ctx.Products.CountAsync());
        Assert.Equal(2, await ctx.Orders.CountAsync());

        Assert.Equal(_world.OrgA.CustomerId, await ctx.Customers.Select(c => c.CustomerId).SingleAsync());
        Assert.Equal(_world.OrgA.SubscriptionId, await ctx.Subscriptions.Select(s => s.SubscriptionId).SingleAsync());

        var orderIds = await ctx.Orders.Select(o => o.OrderId).ToListAsync();
        Assert.DoesNotContain(_world.OrgB.Branch("B1").OrderId, orderIds);
    }

    [Fact]
    public async Task Organization_B_sees_only_its_own_rows()
    {
        await using var ctx = _db.CreateContext(_world.OrgB.OrganizationId);

        Assert.Equal(1, await ctx.Branches.CountAsync());
        Assert.Equal(1, await ctx.Customers.CountAsync());
        Assert.Equal(1, await ctx.Roles.CountAsync());
        Assert.Equal(1, await ctx.Orders.CountAsync());

        Assert.Equal(_world.OrgB.CustomerId, await ctx.Customers.Select(c => c.CustomerId).SingleAsync());
        Assert.Equal(_world.OrgB.Branch("B1").OrderId, await ctx.Orders.Select(o => o.OrderId).SingleAsync());
    }

    [Fact]
    public async Task Two_live_contexts_with_different_tenants_do_not_share_filter_state()
    {
        await using var ctxA = _db.CreateContext(_world.OrgA.OrganizationId);
        await using var ctxB = _db.CreateContext(_world.OrgB.OrganizationId);

        var aNames = await ctxA.Branches.Select(b => b.Name).ToListAsync();
        var bNames = await ctxB.Branches.Select(b => b.Name).ToListAsync();

        Assert.Equal(new[] { "A1", "A2" }, aNames.OrderBy(n => n).ToArray());
        Assert.Equal(new[] { "B1" }, bNames.ToArray());
    }

    [Fact]
    public async Task Find_by_id_of_another_tenants_row_returns_null()
    {
        await using var ctx = _db.CreateContext(_world.OrgA.OrganizationId);

        Assert.Null(await ctx.Branches.FindAsync(_world.OrgB.Branch("B1").BranchId));
        Assert.Null(await ctx.Products.FindAsync(_world.OrgB.Branch("B1").ProductId));
        Assert.Null(await ctx.Orders.FindAsync(_world.OrgB.Branch("B1").OrderId));

        Assert.NotNull(await ctx.Orders.FindAsync(_world.OrgA.Branch("A1").OrderId));
    }

    [Fact]
    public async Task Unfiltered_global_tables_are_visible_to_any_context()
    {
        // Organizations, Users and Permissions are global by design (needed by sign-in and the
        // platform area), so they are deliberately not tenant filtered. This test documents that.
        await using var ctx = _db.CreateContext();

        Assert.Equal(2, await ctx.Organizations.CountAsync());
        Assert.Equal(2, await ctx.Users.CountAsync());
        Assert.Equal(1, await ctx.Permissions.CountAsync());
    }
}
