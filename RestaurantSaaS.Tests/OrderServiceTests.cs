using RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsRequest;
using RestaurantSaaS.Application.DTOs.Orders.OrdersRequest;
using RestaurantSaaS.Application.DTOs.Payments.PaymentsRequest;
using RestaurantSaaS.Application.Services;
using RestaurantSaaS.Infrastructure.Data;
using RestaurantSaaS.Tests.TestSupport;
using Xunit;

namespace RestaurantSaaS.Tests;

/// <summary>
/// Tests for <see cref="OrderService"/>: create an order, add an item, move the status forward,
/// and complete the order only after it is fully paid.
/// </summary>
public sealed class OrderServiceTests : IAsyncLifetime
{
    private readonly TestDatabase _db = new();
    private TestIds _ids = null!;

    public async Task InitializeAsync()
    {
        await _db.CreateAsync();
        _ids = await _db.SeedAsync();
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    // One context and the services that use it, acting like a user of the test organization.
    private (AppDbContext Context, OrderService Orders, OrderItemService Items, PaymentService Payments) CreateServices()
    {
        var tenant = new FakeCurrentTenant { OrganizationId = _ids.OrganizationId };
        var context = _db.CreateContext(_ids.OrganizationId);
        return (context,
                new OrderService(context, tenant),
                new OrderItemService(context),
                new PaymentService(context, tenant));
    }

    private static CreateOrderRequest NewOrder() => new() { OrderType = "Takeaway" };

    [Fact]
    public async Task Creating_an_order_starts_it_as_pending_with_no_total()
    {
        var (context, orders, _, _) = CreateServices();
        await using var _ = context;

        var order = await orders.CreateAsync(_ids.BranchId, NewOrder());

        Assert.Equal("Pending", order.Status);
        Assert.Equal(0m, order.TotalAmount);
        Assert.Equal(_ids.BranchId, order.BranchId);
    }

    [Fact]
    public async Task Adding_an_item_updates_the_order_total()
    {
        var (context, orders, items, _) = CreateServices();
        await using var _ = context;

        var order = await orders.CreateAsync(_ids.BranchId, NewOrder());
        await items.AddAsync(order.OrderId, new CreateOrderItemRequest
        {
            ProductId = _ids.ProductId,
            Quantity = 2m,
            DiscountAmount = 0m
        });

        var updated = await orders.GetByIdAsync(order.OrderId);

        Assert.Equal(20m, updated!.TotalAmount); // 2 x 10.00
        Assert.Single(updated.Items);
    }

    [Fact]
    public async Task The_status_moves_forward()
    {
        var (context, orders, _, _) = CreateServices();
        await using var _ = context;

        var order = await orders.CreateAsync(_ids.BranchId, NewOrder());
        var updated = await orders.UpdateStatusAsync(order.OrderId, new UpdateOrderStatusRequest { Status = "Confirmed" });

        Assert.Equal("Confirmed", updated!.Status);
    }

    [Fact]
    public async Task The_status_cannot_move_backwards()
    {
        var (context, orders, _, _) = CreateServices();
        await using var _ = context;

        var order = await orders.CreateAsync(_ids.BranchId, NewOrder());
        await orders.UpdateStatusAsync(order.OrderId, new UpdateOrderStatusRequest { Status = "Confirmed" });

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orders.UpdateStatusAsync(order.OrderId, new UpdateOrderStatusRequest { Status = "Pending" }));

        Assert.Equal(OrderStatusRules.BackwardStatusMessage, error.Message);
    }

    [Fact]
    public async Task An_order_cannot_be_completed_before_it_is_paid()
    {
        var (context, orders, items, _) = CreateServices();
        await using var _ = context;

        var order = await orders.CreateAsync(_ids.BranchId, NewOrder());
        await items.AddAsync(order.OrderId, new CreateOrderItemRequest
        {
            ProductId = _ids.ProductId,
            Quantity = 2m,
            DiscountAmount = 0m
        });

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => orders.UpdateStatusAsync(order.OrderId, new UpdateOrderStatusRequest { Status = "Completed" }));

        Assert.Contains("Balance due: 20.00", error.Message);
    }

    [Fact]
    public async Task An_order_can_be_completed_after_a_full_cash_payment()
    {
        var (context, orders, items, payments) = CreateServices();
        await using var _ = context;

        var order = await orders.CreateAsync(_ids.BranchId, NewOrder());
        await items.AddAsync(order.OrderId, new CreateOrderItemRequest
        {
            ProductId = _ids.ProductId,
            Quantity = 2m,
            DiscountAmount = 0m
        });
        await payments.CreateAsync(order.OrderId, new CreatePaymentRequest
        {
            PaymentMethod = "Cash",
            Amount = 20m
        });

        var updated = await orders.UpdateStatusAsync(order.OrderId, new UpdateOrderStatusRequest { Status = "Completed" });

        Assert.Equal("Completed", updated!.Status);
        Assert.Equal(20m, updated.PaidAmount);
        Assert.Equal(0m, updated.BalanceDue);
    }
}
