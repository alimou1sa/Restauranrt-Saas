using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs.Orders.OrdersRequest;
using RestaurantSaaS.Application.Services;
using RestaurantSaaS.Infrastructure.Data;
using RestaurantSaaS.Tests.TestSupport;
using Xunit;

namespace RestaurantSaaS.Tests.Isolation;

/// <summary>
/// Isolation at the service layer: the code the controllers actually call must not be able to read
/// or mutate another organization's data, even when it is handed a foreign id directly.
/// </summary>
public sealed class OrderServiceIsolationTests : IAsyncLifetime
{
    private readonly IsolationDatabase _db = new();
    private SeededWorld _world = null!;

    public async Task InitializeAsync()
    {
        await _db.InitializeAsync();
        _world = await TestData.SeedAsync(_db);
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    private (AppDbContext Context, OrderService Service) CreateServiceForA()
    {
        var tenant = new FakeCurrentTenant { OrganizationId = _world.OrgA.OrganizationId };
        var context = new AppDbContext(_db.BuildOptions(), tenant);
        return (context, new OrderService(context, tenant));
    }

    [Fact]
    public async Task GetAll_returns_only_the_current_organization_orders()
    {
        var (ctx, service) = CreateServiceForA();
        await using var _ = ctx;

        var orders = await service.GetAllAsync();

        var expected = new[]
        {
            _world.OrgA.Branch("A1").OrderId,
            _world.OrgA.Branch("A2").OrderId
        };
        Assert.Equal(expected.OrderBy(id => id).ToArray(), orders.Select(o => o.OrderId).OrderBy(id => id).ToArray());
    }

    [Fact]
    public async Task GetById_of_another_organizations_order_returns_null()
    {
        var (ctx, service) = CreateServiceForA();
        await using var _ = ctx;

        Assert.Null(await service.GetByIdAsync(_world.OrgB.Branch("B1").OrderId));
        Assert.NotNull(await service.GetByIdAsync(_world.OrgA.Branch("A1").OrderId));
    }

    [Fact]
    public async Task Create_rejects_a_branch_owned_by_another_organization()
    {
        var (ctx, service) = CreateServiceForA();
        await using var _ = ctx;

        var request = new CreateOrderRequest { OrderType = "DineIn" };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.CreateAsync(_world.OrgB.Branch("B1").BranchId, request));
    }

    [Fact]
    public async Task Create_in_an_own_branch_stamps_the_current_organization()
    {
        var (ctx, service) = CreateServiceForA();
        await using var _ = ctx;

        var created = await service.CreateAsync(_world.OrgA.Branch("A1").BranchId, new CreateOrderRequest { OrderType = "Takeaway" });

        var stored = await ctx.Orders.IgnoreQueryFilters().SingleAsync(o => o.OrderId == created.OrderId);
        Assert.Equal(_world.OrgA.OrganizationId, stored.OrganizationId);
        Assert.Equal(_world.OrgA.Branch("A1").BranchId, stored.BranchId);
    }

    [Fact]
    public async Task Update_cannot_modify_another_organizations_order()
    {
        var (ctx, service) = CreateServiceForA();
        await using var _ = ctx;

        var foreignOrderId = _world.OrgB.Branch("B1").OrderId;
        var result = await service.UpdateAsync(foreignOrderId, new UpdateOrderRequest
        {
            Notes = "cross-tenant write attempt",
            DiscountAmount = 999m
        });

        Assert.Null(result);

        await using var ctxB = _db.CreateContext(_world.OrgB.OrganizationId);
        var untouched = await ctxB.Orders.SingleAsync(o => o.OrderId == foreignOrderId);
        Assert.Null(untouched.Notes);
        Assert.Equal(0m, untouched.DiscountAmount);
    }

    [Fact]
    public async Task Update_status_cannot_change_another_organizations_order()
    {
        var (ctx, service) = CreateServiceForA();
        await using var _ = ctx;

        var foreignOrderId = _world.OrgB.Branch("B1").OrderId;
        var result = await service.UpdateStatusAsync(foreignOrderId, new UpdateOrderStatusRequest { Status = "Canceled" });

        Assert.Null(result);

        await using var ctxB = _db.CreateContext(_world.OrgB.OrganizationId);
        Assert.Equal("Pending", (await ctxB.Orders.SingleAsync(o => o.OrderId == foreignOrderId)).Status);
    }

    [Fact]
    public async Task Delete_cannot_remove_another_organizations_order()
    {
        var (ctx, service) = CreateServiceForA();
        await using var _ = ctx;

        var foreignOrderId = _world.OrgB.Branch("B1").OrderId;
        Assert.False(await service.DeleteAsync(foreignOrderId));

        await using var ctxB = _db.CreateContext(_world.OrgB.OrganizationId);
        Assert.True(await ctxB.Orders.AnyAsync(o => o.OrderId == foreignOrderId));
    }
}
