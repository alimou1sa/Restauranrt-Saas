using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Tests.TestSupport;
using Xunit;

namespace RestaurantSaaS.Tests.Isolation;

/// <summary>
/// Proves the second isolation axis: when a membership is bound to a single branch, that branch's
/// data is the only branch data visible, and a branch id from another organization exposes nothing.
/// </summary>
public sealed class BranchScopeTests : IAsyncLifetime
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
    public async Task A_branch_bound_context_sees_only_that_branch_data_but_the_full_branch_list()
    {
        var a1 = _world.OrgA.Branch("A1");
        await using var ctx = _db.CreateContext(_world.OrgA.OrganizationId, branchId: a1.BranchId);

        // Branches are not branch filtered (you must be able to see the branch switcher list)...
        Assert.Equal(2, await ctx.Branches.CountAsync());

        // ...but every branch owned row is limited to branch A1.
        Assert.Equal(1, await ctx.Menus.CountAsync());
        Assert.Equal(a1.MenuId, await ctx.Menus.Select(m => m.MenuId).SingleAsync());
        Assert.Equal(1, await ctx.Categories.CountAsync());
        Assert.Equal(1, await ctx.Products.CountAsync());
        Assert.Equal(1, await ctx.RestaurantTables.CountAsync());
        Assert.Equal(1, await ctx.Orders.CountAsync());
        Assert.Equal(1, await ctx.OrderItems.CountAsync());
        Assert.Equal(1, await ctx.Payments.CountAsync());
        Assert.Equal(1, await ctx.Inventories.CountAsync());
        Assert.Equal(1, await ctx.InventoryTransactions.CountAsync());

        Assert.Equal(a1.OrderId, await ctx.Orders.Select(o => o.OrderId).SingleAsync());
        Assert.Equal(a1.ProductId, await ctx.Products.Select(p => p.ProductId).SingleAsync());
    }

    [Fact]
    public async Task Branch_boundaries_are_separate_per_branch_in_the_same_organization()
    {
        var a2 = _world.OrgA.Branch("A2");
        await using var ctx = _db.CreateContext(_world.OrgA.OrganizationId, branchId: a2.BranchId);

        Assert.Equal(1, await ctx.Menus.CountAsync());
        Assert.Equal(a2.MenuId, await ctx.Menus.Select(m => m.MenuId).SingleAsync());
        Assert.Equal(a2.OrderId, await ctx.Orders.Select(o => o.OrderId).SingleAsync());
    }

    [Fact]
    public async Task A_branch_id_from_another_organization_exposes_nothing()
    {
        // Wrong pairing that a broken implementation might trust: org A context + org B branch id.
        await using var ctx = _db.CreateContext(_world.OrgA.OrganizationId, branchId: _world.OrgB.Branch("B1").BranchId);

        Assert.Equal(0, await ctx.Menus.CountAsync());
        Assert.Equal(0, await ctx.Categories.CountAsync());
        Assert.Equal(0, await ctx.Products.CountAsync());
        Assert.Equal(0, await ctx.RestaurantTables.CountAsync());
        Assert.Equal(0, await ctx.Orders.CountAsync());
        Assert.Equal(0, await ctx.OrderItems.CountAsync());
        Assert.Equal(0, await ctx.Payments.CountAsync());
        Assert.Equal(0, await ctx.Inventories.CountAsync());
        Assert.Equal(0, await ctx.InventoryTransactions.CountAsync());
    }

    [Fact]
    public async Task Without_a_branch_restriction_every_branch_of_the_organization_is_visible()
    {
        await using var ctx = _db.CreateContext(_world.OrgA.OrganizationId);

        Assert.Equal(2, await ctx.Menus.CountAsync());
        Assert.Equal(2, await ctx.Orders.CountAsync());
        Assert.Equal(2, await ctx.Products.CountAsync());
        Assert.Equal(2, await ctx.Inventories.CountAsync());
    }
}
