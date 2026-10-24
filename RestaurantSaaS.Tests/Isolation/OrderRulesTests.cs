using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsRequest;
using RestaurantSaaS.Application.DTOs.Orders.OrdersRequest;
using RestaurantSaaS.Application.Services;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Infrastructure.Data;
using RestaurantSaaS.Tests.TestSupport;
using Xunit;

namespace RestaurantSaaS.Tests.Isolation;

/// <summary>
/// P1 order rules: items are frozen on closed orders, statuses only move forward (Canceled from any open status),
/// closed orders cannot be edited, and only Pending orders without payments can be deleted. Every rule is also
/// checked from the other tenant's side: a foreign order is invisible, so it yields null/false/NotFound and never
/// leaks its status through a business-rule error.
/// </summary>
public sealed class OrderRulesTests : IAsyncLifetime
{
    private readonly IsolationDatabase _db = new();
    private SeededWorld _world = null!;

    public async Task InitializeAsync()
    {
        await _db.InitializeAsync();
        _world = await TestData.SeedAsync(_db);
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    private BranchSeed BranchA => _world.OrgA.Branch("A1");
    private BranchSeed BranchB => _world.OrgB.Branch("B1");

    // Seeds an order directly (no tenant context, like TestData does) so any status can be set up.
    private async Task<(int OrderId, int ItemId)> SeedOrderAsync(OrgSeed org, BranchSeed branch, string status, bool withPayment = false, bool paidInFull = false)
    {
        await using var ctx = _db.CreateContext();
        var order = new Order
        {
            OrganizationId = org.OrganizationId,
            BranchId = branch.BranchId,
            OrderNumber = "ORD-RULES-" + Guid.NewGuid().ToString("N")[..12],
            Status = status,
            OrderType = "Takeaway",
            SubTotal = 20m,
            TotalAmount = 20m,
            CreatedAtUtc = TestData.Now,
            ClosedAtUtc = OrderStatusRules.IsClosed(status) ? TestData.Now : null
        };
        ctx.Add(order);
        await ctx.SaveChangesAsync();

        var item = new OrderItem
        {
            OrderId = order.OrderId,
            ProductId = branch.ProductId,
            ProductName = "Seeded",
            UnitPrice = 10m,
            Quantity = 2m,
            DiscountAmount = 0m
        };
        ctx.Add(item);
        if (withPayment)
            ctx.Add(new Payment { OrderId = order.OrderId, Amount = 5m, PaymentMethod = "Cash", Status = "Completed", CreatedAtUtc = TestData.Now });
        if (paidInFull) // completing an order requires Completed payments to cover the total (P2)
            ctx.Add(new Payment { OrderId = order.OrderId, Amount = 20m, PaymentMethod = "Cash", Status = "Completed", CreatedAtUtc = TestData.Now });
        await ctx.SaveChangesAsync();
        return (order.OrderId, item.OrderItemId);
    }

    private (AppDbContext Context, OrderService Orders, OrderItemService Items) ServicesForA()
    {
        var tenant = new FakeCurrentTenant { OrganizationId = _world.OrgA.OrganizationId };
        var ctx = new AppDbContext(_db.BuildOptions(), tenant);
        return (ctx, new OrderService(ctx, tenant), new OrderItemService(ctx));
    }

    private async Task<Order> ReloadAsync(OrgSeed org, int orderId)
    {
        await using var ctx = _db.CreateContext(org.OrganizationId);
        return await ctx.Orders.AsNoTracking().SingleAsync(o => o.OrderId == orderId);
    }

    // ---------------------------------------------------------------- order items

    [Theory]
    [InlineData("Completed")]
    [InlineData("Canceled")]
    public async Task Adding_an_item_to_a_closed_order_is_rejected(string status)
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, status);
        var (ctx, _, items) = ServicesForA();
        await using var _ = ctx;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => items.AddAsync(orderId, new CreateOrderItemRequest { ProductId = BranchA.ProductId, Quantity = 1m, DiscountAmount = 0m }));

        Assert.Equal(OrderStatusRules.ClosedOrderItemsMessage, ex.Message);
        Assert.Equal(1, await _db.CreateContext(_world.OrgA.OrganizationId).OrderItems.CountAsync(i => i.OrderId == orderId));
        Assert.Equal(20m, (await ReloadAsync(_world.OrgA, orderId)).TotalAmount);
    }

    [Theory]
    [InlineData("Completed")]
    [InlineData("Canceled")]
    public async Task Updating_an_item_of_a_closed_order_is_rejected(string status)
    {
        var (orderId, itemId) = await SeedOrderAsync(_world.OrgA, BranchA, status);
        var (ctx, _, items) = ServicesForA();
        await using var _ = ctx;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => items.UpdateAsync(itemId, new UpdateOrderItemRequest { Quantity = 9m, DiscountAmount = 0m }));

        Assert.Equal(OrderStatusRules.ClosedOrderItemsMessage, ex.Message);
        await using var check = _db.CreateContext(_world.OrgA.OrganizationId);
        Assert.Equal(2m, (await check.OrderItems.SingleAsync(i => i.OrderItemId == itemId)).Quantity);
        Assert.Equal(20m, (await ReloadAsync(_world.OrgA, orderId)).TotalAmount);
    }

    [Theory]
    [InlineData("Completed")]
    [InlineData("Canceled")]
    public async Task Removing_an_item_of_a_closed_order_is_rejected(string status)
    {
        var (orderId, itemId) = await SeedOrderAsync(_world.OrgA, BranchA, status);
        var (ctx, _, items) = ServicesForA();
        await using var _ = ctx;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => items.RemoveAsync(itemId));

        Assert.Equal(OrderStatusRules.ClosedOrderItemsMessage, ex.Message);
        await using var check = _db.CreateContext(_world.OrgA.OrganizationId);
        Assert.True(await check.OrderItems.AnyAsync(i => i.OrderItemId == itemId));
        Assert.Equal(20m, (await ReloadAsync(_world.OrgA, orderId)).TotalAmount);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Confirmed")]
    [InlineData("Preparing")]
    [InlineData("Ready")]
    [InlineData("Served")]
    public async Task Items_can_still_be_added_updated_and_removed_while_the_order_is_open(string status)
    {
        var (orderId, itemId) = await SeedOrderAsync(_world.OrgA, BranchA, status);
        var (ctx, _, items) = ServicesForA();
        await using var _ = ctx;

        var added = await items.AddAsync(orderId, new CreateOrderItemRequest { ProductId = BranchA.ProductId, Quantity = 1m, DiscountAmount = 0m });
        Assert.NotNull(await items.UpdateAsync(itemId, new UpdateOrderItemRequest { Quantity = 3m, DiscountAmount = 0m }));
        Assert.True(await items.RemoveAsync(added.OrderItemId));

        Assert.Equal(30m, (await ReloadAsync(_world.OrgA, orderId)).TotalAmount);
    }

    [Fact]
    public async Task Item_operations_on_another_organizations_order_do_not_leak_or_change_anything()
    {
        // The foreign order is closed on purpose: the caller must still see "not found", not a rule violation
        // that would reveal the other tenant's order state.
        var (foreignOrderId, foreignItemId) = await SeedOrderAsync(_world.OrgB, BranchB, "Completed");
        var (ctx, _, items) = ServicesForA();
        await using var _ = ctx;

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => items.AddAsync(foreignOrderId, new CreateOrderItemRequest { ProductId = BranchA.ProductId, Quantity = 1m, DiscountAmount = 0m }));
        Assert.Null(await items.UpdateAsync(foreignItemId, new UpdateOrderItemRequest { Quantity = 9m, DiscountAmount = 0m }));
        Assert.False(await items.RemoveAsync(foreignItemId));

        await using var check = _db.CreateContext(_world.OrgB.OrganizationId);
        Assert.Equal(2m, (await check.OrderItems.SingleAsync(i => i.OrderItemId == foreignItemId)).Quantity);
    }

    // ---------------------------------------------------------------- status machine

    [Theory]
    [InlineData("Pending", "Confirmed")]
    [InlineData("Pending", "Preparing")]
    [InlineData("Pending", "Completed")] // takeaway: skipping forward is allowed
    [InlineData("Confirmed", "Served")]
    [InlineData("Preparing", "Ready")]
    [InlineData("Ready", "Completed")]
    [InlineData("Served", "Completed")]
    public async Task Status_can_move_forward_and_skip_forward(string from, string to)
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, from, paidInFull: to == "Completed");
        var (ctx, orders, _) = ServicesForA();
        await using var _ = ctx;

        var result = await orders.UpdateStatusAsync(orderId, new UpdateOrderStatusRequest { Status = to });

        Assert.Equal(to, result!.Status);
        Assert.Equal(to, (await ReloadAsync(_world.OrgA, orderId)).Status);
    }

    [Theory]
    [InlineData("Confirmed", "Pending")]
    [InlineData("Preparing", "Pending")]
    [InlineData("Ready", "Confirmed")]
    [InlineData("Served", "Preparing")]
    [InlineData("Pending", "Pending")]
    [InlineData("Ready", "Ready")]
    public async Task Status_cannot_move_backward_or_stay_the_same(string from, string to)
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, from);
        var (ctx, orders, _) = ServicesForA();
        await using var _ = ctx;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orders.UpdateStatusAsync(orderId, new UpdateOrderStatusRequest { Status = to }));

        Assert.Equal(OrderStatusRules.BackwardStatusMessage, ex.Message);
        Assert.Equal(from, (await ReloadAsync(_world.OrgA, orderId)).Status);
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Confirmed")]
    [InlineData("Preparing")]
    [InlineData("Ready")]
    [InlineData("Served")]
    public async Task Canceled_is_allowed_from_every_open_status_and_stamps_ClosedAtUtc(string from)
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, from);
        var (ctx, orders, _) = ServicesForA();
        await using var _ = ctx;

        var result = await orders.UpdateStatusAsync(orderId, new UpdateOrderStatusRequest { Status = "Canceled" });

        Assert.Equal("Canceled", result!.Status);
        Assert.NotNull(result.ClosedAtUtc);
    }

    [Theory]
    [InlineData("Completed", "Canceled")]
    [InlineData("Completed", "Pending")]
    [InlineData("Completed", "Completed")]
    [InlineData("Canceled", "Pending")]
    [InlineData("Canceled", "Completed")]
    [InlineData("Canceled", "Canceled")]
    public async Task A_closed_order_cannot_change_status_and_ClosedAtUtc_is_untouched(string from, string to)
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, from);
        var (ctx, orders, _) = ServicesForA();
        await using var _ = ctx;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orders.UpdateStatusAsync(orderId, new UpdateOrderStatusRequest { Status = to }));

        Assert.Equal(OrderStatusRules.ClosedOrderStatusMessage, ex.Message);
        var stored = await ReloadAsync(_world.OrgA, orderId);
        Assert.Equal(from, stored.Status);
        Assert.Equal(TestData.Now, stored.ClosedAtUtc);
    }

    [Fact]
    public async Task ClosedAtUtc_is_set_when_the_order_is_completed_and_stays_empty_before()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, "Pending", paidInFull: true);
        var (ctx, orders, _) = ServicesForA();
        await using var _ = ctx;

        var served = await orders.UpdateStatusAsync(orderId, new UpdateOrderStatusRequest { Status = "Served" });
        Assert.Null(served!.ClosedAtUtc);

        var completed = await orders.UpdateStatusAsync(orderId, new UpdateOrderStatusRequest { Status = "Completed" });
        Assert.NotNull(completed!.ClosedAtUtc);
    }

    [Fact]
    public async Task An_unknown_status_is_rejected()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, "Pending");
        var (ctx, orders, _) = ServicesForA();
        await using var _ = ctx;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => orders.UpdateStatusAsync(orderId, new UpdateOrderStatusRequest { Status = "Shipped" }));
        Assert.Equal("Pending", (await ReloadAsync(_world.OrgA, orderId)).Status);
    }

    [Fact]
    public async Task Status_of_another_organizations_closed_order_returns_null_without_leaking_the_rule()
    {
        var (foreignOrderId, _) = await SeedOrderAsync(_world.OrgB, BranchB, "Completed");
        var (ctx, orders, _) = ServicesForA();
        await using var _ = ctx;

        Assert.Null(await orders.UpdateStatusAsync(foreignOrderId, new UpdateOrderStatusRequest { Status = "Canceled" }));
        Assert.Equal("Completed", (await ReloadAsync(_world.OrgB, foreignOrderId)).Status);
    }

    // ---------------------------------------------------------------- update (PUT)

    [Theory]
    [InlineData("Completed")]
    [InlineData("Canceled")]
    public async Task Updating_a_closed_order_is_rejected(string status)
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, status);
        var (ctx, orders, _) = ServicesForA();
        await using var _ = ctx;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orders.UpdateAsync(orderId, new UpdateOrderRequest { Notes = "late edit", DiscountAmount = 5m }));

        Assert.Equal(OrderStatusRules.ClosedOrderModifyMessage, ex.Message);
        var stored = await ReloadAsync(_world.OrgA, orderId);
        Assert.Null(stored.Notes);
        Assert.Equal(0m, stored.DiscountAmount);
        Assert.Equal(20m, stored.TotalAmount);
    }

    [Fact]
    public async Task Updating_an_open_order_still_works()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, "Preparing");
        var (ctx, orders, _) = ServicesForA();
        await using var _ = ctx;

        var result = await orders.UpdateAsync(orderId, new UpdateOrderRequest { Notes = "no onions", DiscountAmount = 5m });

        Assert.Equal("no onions", result!.Notes);
        Assert.Equal(15m, result.TotalAmount);
    }

    [Fact]
    public async Task Updating_another_organizations_closed_order_returns_null()
    {
        var (foreignOrderId, _) = await SeedOrderAsync(_world.OrgB, BranchB, "Canceled");
        var (ctx, orders, _) = ServicesForA();
        await using var _ = ctx;

        Assert.Null(await orders.UpdateAsync(foreignOrderId, new UpdateOrderRequest { Notes = "x", DiscountAmount = 1m }));
        Assert.Null((await ReloadAsync(_world.OrgB, foreignOrderId)).Notes);
    }

    // ---------------------------------------------------------------- delete

    [Fact]
    public async Task A_pending_order_without_payments_can_be_deleted_together_with_its_items()
    {
        var (orderId, itemId) = await SeedOrderAsync(_world.OrgA, BranchA, "Pending");
        var (ctx, orders, _) = ServicesForA();
        await using var _ = ctx;

        Assert.True(await orders.DeleteAsync(orderId));

        await using var check = _db.CreateContext(_world.OrgA.OrganizationId);
        Assert.False(await check.Orders.AnyAsync(o => o.OrderId == orderId));
        Assert.False(await check.OrderItems.AnyAsync(i => i.OrderItemId == itemId));
    }

    [Theory]
    [InlineData("Completed")]
    [InlineData("Canceled")]
    public async Task A_closed_order_cannot_be_deleted(string status)
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, status);
        var (ctx, orders, _) = ServicesForA();
        await using var _ = ctx;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => orders.DeleteAsync(orderId));

        Assert.Equal(OrderStatusRules.ClosedOrderDeleteMessage, ex.Message);
        Assert.Equal(status, (await ReloadAsync(_world.OrgA, orderId)).Status);
    }

    [Theory]
    [InlineData("Confirmed")]
    [InlineData("Preparing")]
    [InlineData("Ready")]
    [InlineData("Served")]
    public async Task A_started_order_cannot_be_deleted_it_must_be_canceled(string status)
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, status);
        var (ctx, orders, _) = ServicesForA();
        await using var _ = ctx;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => orders.DeleteAsync(orderId));

        Assert.Equal(OrderStatusRules.DeleteOnlyPendingMessage, ex.Message);
        Assert.Equal(status, (await ReloadAsync(_world.OrgA, orderId)).Status);
    }

    [Fact]
    public async Task A_pending_order_with_a_payment_cannot_be_deleted()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, "Pending", withPayment: true);
        var (ctx, orders, _) = ServicesForA();
        await using var _ = ctx;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => orders.DeleteAsync(orderId));

        Assert.Equal(OrderStatusRules.DeleteWithPaymentsMessage, ex.Message);
        Assert.Equal("Pending", (await ReloadAsync(_world.OrgA, orderId)).Status);
    }

    [Fact]
    public async Task Deleting_another_organizations_order_returns_false_whatever_its_state()
    {
        var (closedId, _) = await SeedOrderAsync(_world.OrgB, BranchB, "Completed");
        var (pendingId, _) = await SeedOrderAsync(_world.OrgB, BranchB, "Pending");
        var (ctx, orders, _) = ServicesForA();
        await using var _ = ctx;

        Assert.False(await orders.DeleteAsync(closedId));
        Assert.False(await orders.DeleteAsync(pendingId));

        await using var check = _db.CreateContext(_world.OrgB.OrganizationId);
        Assert.True(await check.Orders.AnyAsync(o => o.OrderId == closedId));
        Assert.True(await check.Orders.AnyAsync(o => o.OrderId == pendingId));
    }

    // ---------------------------------------------------------------- the rule table itself

    [Theory]
    [InlineData("Pending", "Canceled", true)]
    [InlineData("Served", "Canceled", true)]
    [InlineData("Completed", "Canceled", false)]
    [InlineData("Canceled", "Canceled", false)]
    [InlineData("Pending", "Completed", true)]
    [InlineData("Completed", "Served", false)]
    [InlineData("Served", "Pending", false)]
    [InlineData("Bogus", "Completed", false)]
    public void CanMove_matches_the_documented_rules(string from, string to, bool expected)
        => Assert.Equal(expected, OrderStatusRules.CanMove(from, to));
}
