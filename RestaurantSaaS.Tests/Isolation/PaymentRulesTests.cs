using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsRequest;
using RestaurantSaaS.Application.DTOs.Orders.OrdersRequest;
using RestaurantSaaS.Application.DTOs.Payments.PaymentsRequest;
using RestaurantSaaS.Application.Services;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Infrastructure.Data;
using RestaurantSaaS.Tests.TestSupport;
using Xunit;

namespace RestaurantSaaS.Tests.Isolation;

/// <summary>
/// P2 payment rules: methods, amount and reference validation, duplicate references, overpayment, the status
/// transitions, who created/confirmed a payment, the effect of payments on order items and on completing an order,
/// and the PaidAmount/BalanceDue figures. Every rule that reads or writes payments is also checked from the other
/// tenant's side.
/// </summary>
public sealed class PaymentRulesTests : IAsyncLifetime
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

    // Seeds an order of 20.00 (one item, 2 x 10.00) with the given payments, bypassing the services.
    private async Task<(int OrderId, int ItemId)> SeedOrderAsync(OrgSeed org, BranchSeed branch, string status = "Pending",
        params (decimal Amount, string Status, string Method, string? Reference)[] payments)
    {
        await using var ctx = _db.CreateContext();
        var order = new Order
        {
            OrganizationId = org.OrganizationId,
            BranchId = branch.BranchId,
            OrderNumber = "ORD-PAY-" + Guid.NewGuid().ToString("N")[..12],
            Status = status,
            OrderType = "Takeaway",
            SubTotal = 20m,
            TotalAmount = 20m,
            CreatedAtUtc = TestData.Now,
            ClosedAtUtc = OrderStatusRules.IsClosed(status) ? TestData.Now : null
        };
        ctx.Add(order);
        await ctx.SaveChangesAsync();

        var item = new OrderItem { OrderId = order.OrderId, ProductId = branch.ProductId, ProductName = "Seeded", UnitPrice = 10m, Quantity = 2m, DiscountAmount = 0m };
        ctx.Add(item);
        foreach (var p in payments)
            ctx.Add(new Payment { OrderId = order.OrderId, Amount = p.Amount, Status = p.Status, PaymentMethod = p.Method, TransactionReference = p.Reference, CreatedAtUtc = TestData.Now });
        await ctx.SaveChangesAsync();
        return (order.OrderId, item.OrderItemId);
    }

    private async Task<int> SeedZeroTotalOrderAsync()
    {
        await using var ctx = _db.CreateContext();
        var order = new Order
        {
            OrganizationId = _world.OrgA.OrganizationId,
            BranchId = BranchA.BranchId,
            OrderNumber = "ORD-ZERO-" + Guid.NewGuid().ToString("N")[..12],
            Status = "Served",
            OrderType = "Takeaway",
            CreatedAtUtc = TestData.Now
        };
        ctx.Add(order);
        await ctx.SaveChangesAsync();
        return order.OrderId;
    }

    private sealed record Services(AppDbContext Context, PaymentService Payments, OrderService Orders, OrderItemService Items) : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => Context.DisposeAsync();
    }

    private Services ServicesFor(OrgSeed org)
    {
        var tenant = new FakeCurrentTenant { OrganizationId = org.OrganizationId, OrganizationUserId = org.OrganizationUserId };
        var ctx = new AppDbContext(_db.BuildOptions(), tenant);
        return new Services(ctx, new PaymentService(ctx, tenant), new OrderService(ctx, tenant), new OrderItemService(ctx));
    }

    private static CreatePaymentRequest Pay(string method, decimal amount, string? reference = null) =>
        new() { PaymentMethod = method, Amount = amount, TransactionReference = reference };

    private async Task<Payment> ReloadPaymentAsync(OrgSeed org, int paymentId)
    {
        await using var ctx = _db.CreateContext(org.OrganizationId);
        return await ctx.Payments.AsNoTracking().SingleAsync(p => p.PaymentId == paymentId);
    }

    private async Task<int> PaymentCountAsync(OrgSeed org, int orderId)
    {
        await using var ctx = _db.CreateContext(org.OrganizationId);
        return await ctx.Payments.CountAsync(p => p.OrderId == orderId);
    }

    // ---------------------------------------------------------------- methods

    [Theory]
    [InlineData("ShamCash")]
    [InlineData("SyriatelCash")]
    [InlineData("MTNCash")]
    [InlineData("Card")]
    [InlineData("Online")]
    [InlineData("Other")]
    public async Task Non_cash_methods_are_created_Pending_and_record_who_created_them(string method)
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);

        var payment = await s.Payments.CreateAsync(orderId, Pay(method, 5m, "REF-" + method));

        Assert.Equal("Pending", payment.Status);
        Assert.Null(payment.PaidAtUtc);
        Assert.Equal("REF-" + method, payment.TransactionReference);
        Assert.Equal(_world.OrgA.OrganizationUserId, payment.CreatedByOrganizationUserId);
        Assert.Null(payment.ConfirmedByOrganizationUserId);
    }

    [Fact]
    public async Task Cash_is_Completed_at_once_and_confirmed_by_the_creator()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);

        var payment = await s.Payments.CreateAsync(orderId, Pay("Cash", 5m));

        Assert.Equal("Completed", payment.Status);
        Assert.NotNull(payment.PaidAtUtc);
        Assert.Equal(_world.OrgA.OrganizationUserId, payment.CreatedByOrganizationUserId);
        Assert.Equal(_world.OrgA.OrganizationUserId, payment.ConfirmedByOrganizationUserId);
    }

    [Fact]
    public async Task An_unknown_method_is_rejected()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);

        await Assert.ThrowsAsync<InvalidOperationException>(() => s.Payments.CreateAsync(orderId, Pay("Bitcoin", 5m, "x")));
        Assert.Equal(0, await PaymentCountAsync(_world.OrgA, orderId));
    }

    // ---------------------------------------------------------------- amount and reference

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    public async Task Amount_must_be_greater_than_zero(string amount)
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => s.Payments.CreateAsync(orderId, Pay("Cash", decimal.Parse(amount))));

        Assert.Equal(PaymentRules.AmountMustBePositiveMessage, ex.Message);
        Assert.Equal(0, await PaymentCountAsync(_world.OrgA, orderId));
    }

    [Fact]
    public async Task Amount_with_more_than_two_decimals_is_rejected_not_rounded()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => s.Payments.CreateAsync(orderId, Pay("Cash", 5.005m)));

        Assert.Equal(PaymentRules.AmountDecimalsMessage, ex.Message);
    }

    [Theory]
    [InlineData("ShamCash", null)]
    [InlineData("SyriatelCash", "")]
    [InlineData("MTNCash", "   ")]
    [InlineData("Card", null)]
    public async Task Non_cash_methods_require_a_transaction_reference(string method, string? reference)
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => s.Payments.CreateAsync(orderId, Pay(method, 5m, reference)));

        Assert.Equal(PaymentRules.ReferenceRequiredMessage, ex.Message);
        Assert.Equal(0, await PaymentCountAsync(_world.OrgA, orderId));
    }

    [Fact]
    public async Task Cash_does_not_need_a_reference()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);

        var payment = await s.Payments.CreateAsync(orderId, Pay("Cash", 5m));

        Assert.Null(payment.TransactionReference);
    }

    [Fact]
    public async Task The_reference_is_trimmed_before_it_is_stored()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);

        var payment = await s.Payments.CreateAsync(orderId, Pay("ShamCash", 5m, "  TX-1  "));

        Assert.Equal("TX-1", payment.TransactionReference);
    }

    // ---------------------------------------------------------------- duplicate references

    [Fact]
    public async Task The_same_reference_cannot_be_recorded_twice_for_a_method_even_on_another_order()
    {
        var (first, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        var (second, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);

        await s.Payments.CreateAsync(first, Pay("ShamCash", 5m, "TX-100"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => s.Payments.CreateAsync(second, Pay("ShamCash", 5m, " TX-100 ")));
        Assert.Equal(PaymentRules.ReferenceDuplicateMessage, ex.Message);
        Assert.Equal(0, await PaymentCountAsync(_world.OrgA, second));
    }

    [Fact]
    public async Task The_same_reference_is_allowed_for_a_different_method()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);

        await s.Payments.CreateAsync(orderId, Pay("ShamCash", 5m, "TX-200"));
        var other = await s.Payments.CreateAsync(orderId, Pay("SyriatelCash", 5m, "TX-200"));

        Assert.Equal("SyriatelCash", other.PaymentMethod);
    }

    [Fact]
    public async Task A_failed_payments_reference_can_be_recorded_again()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, "Pending", (5m, "Failed", "ShamCash", "TX-300"));
        await using var s = ServicesFor(_world.OrgA);

        var again = await s.Payments.CreateAsync(orderId, Pay("ShamCash", 5m, "TX-300"));

        Assert.Equal("Pending", again.Status);
    }

    [Fact]
    public async Task References_are_unique_per_organization_not_globally()
    {
        // Organization B already recorded TX-400; organization A must neither see it nor be blocked by it.
        await SeedOrderAsync(_world.OrgB, BranchB, "Pending", (5m, "Pending", "ShamCash", "TX-400"));
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);

        var payment = await s.Payments.CreateAsync(orderId, Pay("ShamCash", 5m, "TX-400"));

        Assert.Equal("Pending", payment.Status);
    }

    // ---------------------------------------------------------------- order state and balance

    [Fact]
    public async Task Payments_on_a_canceled_order_are_rejected()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, "Canceled");
        await using var s = ServicesFor(_world.OrgA);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => s.Payments.CreateAsync(orderId, Pay("Cash", 5m)));

        Assert.Equal(PaymentRules.OrderCanceledMessage, ex.Message);
        Assert.Equal(0, await PaymentCountAsync(_world.OrgA, orderId));
    }

    [Fact]
    public async Task Completed_plus_Pending_payments_cannot_exceed_the_order_total()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, "Pending",
            (10m, "Completed", "Cash", null), (5m, "Pending", "ShamCash", "TX-500"));
        await using var s = ServicesFor(_world.OrgA);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => s.Payments.CreateAsync(orderId, Pay("Cash", 5.01m)));
        Assert.Equal(PaymentRules.ExceedsBalanceMessage(5m), ex.Message);

        var exact = await s.Payments.CreateAsync(orderId, Pay("Cash", 5m)); // exactly the remaining balance is fine
        Assert.Equal("Completed", exact.Status);

        var full = await Assert.ThrowsAsync<InvalidOperationException>(() => s.Payments.CreateAsync(orderId, Pay("Cash", 0.01m)));
        Assert.Equal(PaymentRules.ExceedsBalanceMessage(0m), full.Message);
    }

    [Fact]
    public async Task Failed_and_Refunded_payments_do_not_count_against_the_balance()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, "Pending",
            (20m, "Failed", "ShamCash", "TX-600"), (20m, "Refunded", "Cash", null));
        await using var s = ServicesFor(_world.OrgA);

        var payment = await s.Payments.CreateAsync(orderId, Pay("Cash", 20m));

        Assert.Equal("Completed", payment.Status);
    }

    // ---------------------------------------------------------------- status transitions

    [Fact]
    public async Task Confirming_a_pending_payment_stamps_PaidAtUtc_and_who_confirmed_it()
    {
        await SeedOrderAsync(_world.OrgA, BranchA);
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);
        var created = await s.Payments.CreateAsync(orderId, Pay("ShamCash", 5m, "TX-700"));

        var confirmed = await s.Payments.UpdateStatusAsync((int)created.PaymentId, new UpdatePaymentStatusRequest { Status = "Completed" });

        Assert.Equal("Completed", confirmed!.Status);
        Assert.NotNull(confirmed.PaidAtUtc);
        Assert.Equal(_world.OrgA.OrganizationUserId, confirmed.ConfirmedByOrganizationUserId);
        Assert.Equal(_world.OrgA.OrganizationUserId, confirmed.CreatedByOrganizationUserId);
    }

    [Fact]
    public async Task A_pending_payment_can_fail_and_a_completed_one_can_be_refunded()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);
        var pending = await s.Payments.CreateAsync(orderId, Pay("ShamCash", 5m, "TX-800"));
        var cash = await s.Payments.CreateAsync(orderId, Pay("Cash", 5m));

        var failed = await s.Payments.UpdateStatusAsync((int)pending.PaymentId, new UpdatePaymentStatusRequest { Status = "Failed" });
        var refunded = await s.Payments.UpdateStatusAsync((int)cash.PaymentId, new UpdatePaymentStatusRequest { Status = "Refunded" });

        Assert.Equal("Failed", failed!.Status);
        Assert.Null(failed.ConfirmedByOrganizationUserId);
        Assert.Equal("Refunded", refunded!.Status);
    }

    [Theory]
    [InlineData("Pending", "Pending")]
    [InlineData("Pending", "Refunded")]
    [InlineData("Completed", "Pending")]
    [InlineData("Completed", "Failed")]
    [InlineData("Completed", "Completed")]
    [InlineData("Failed", "Pending")]
    [InlineData("Failed", "Completed")]
    [InlineData("Failed", "Refunded")]
    [InlineData("Refunded", "Pending")]
    [InlineData("Refunded", "Completed")]
    [InlineData("Refunded", "Failed")]
    public async Task Every_other_status_change_is_rejected_and_leaves_the_payment_untouched(string from, string to)
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, "Pending", (5m, from, "ShamCash", "TX-900"));
        int paymentId;
        await using (var ctx = _db.CreateContext(_world.OrgA.OrganizationId))
            paymentId = await ctx.Payments.Where(p => p.OrderId == orderId).Select(p => p.PaymentId).SingleAsync();
        await using var s = ServicesFor(_world.OrgA);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => s.Payments.UpdateStatusAsync(paymentId, new UpdatePaymentStatusRequest { Status = to }));

        Assert.Equal(PaymentRules.TransitionMessage(from, to), ex.Message);
        var stored = await ReloadPaymentAsync(_world.OrgA, paymentId);
        Assert.Equal(from, stored.Status);
        Assert.Null(stored.ConfirmedByOrganizationUserId);
    }

    [Fact]
    public async Task An_unknown_payment_status_is_rejected()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);
        var created = await s.Payments.CreateAsync(orderId, Pay("Cash", 5m));

        await Assert.ThrowsAsync<InvalidOperationException>(() => s.Payments.UpdateStatusAsync((int)created.PaymentId, new UpdatePaymentStatusRequest { Status = "Paid" }));
    }

    // ---------------------------------------------------------------- order items and completion

    [Fact]
    public async Task Items_are_frozen_once_the_order_has_a_Completed_payment()
    {
        var (orderId, itemId) = await SeedOrderAsync(_world.OrgA, BranchA, "Preparing", (5m, "Completed", "Cash", null));
        await using var s = ServicesFor(_world.OrgA);

        var add = await Assert.ThrowsAsync<InvalidOperationException>(
            () => s.Items.AddAsync(orderId, new CreateOrderItemRequest { ProductId = BranchA.ProductId, Quantity = 1m, DiscountAmount = 0m }));
        var update = await Assert.ThrowsAsync<InvalidOperationException>(
            () => s.Items.UpdateAsync(itemId, new UpdateOrderItemRequest { Quantity = 9m, DiscountAmount = 0m }));
        var remove = await Assert.ThrowsAsync<InvalidOperationException>(() => s.Items.RemoveAsync(itemId));

        Assert.All(new[] { add, update, remove }, ex => Assert.Equal(OrderStatusRules.ItemsHavePaymentsMessage, ex.Message));
        await using var check = _db.CreateContext(_world.OrgA.OrganizationId);
        Assert.Equal(2m, (await check.OrderItems.SingleAsync(i => i.OrderItemId == itemId)).Quantity);
        Assert.Equal(20m, (await check.Orders.SingleAsync(o => o.OrderId == orderId)).TotalAmount);
    }

    [Fact]
    public async Task Pending_Failed_and_Refunded_payments_do_not_freeze_the_items()
    {
        var (orderId, itemId) = await SeedOrderAsync(_world.OrgA, BranchA, "Preparing",
            (5m, "Pending", "ShamCash", "TX-A1"), (5m, "Failed", "ShamCash", "TX-A2"), (5m, "Refunded", "Cash", null));
        await using var s = ServicesFor(_world.OrgA);

        Assert.NotNull(await s.Items.UpdateAsync(itemId, new UpdateOrderItemRequest { Quantity = 3m, DiscountAmount = 0m }));
        var added = await s.Items.AddAsync(orderId, new CreateOrderItemRequest { ProductId = BranchA.ProductId, Quantity = 1m, DiscountAmount = 0m });
        Assert.True(await s.Items.RemoveAsync(added.OrderItemId));
    }

    [Fact]
    public async Task Refunding_the_only_Completed_payment_unfreezes_the_items()
    {
        var (orderId, itemId) = await SeedOrderAsync(_world.OrgA, BranchA, "Preparing");
        await using var s = ServicesFor(_world.OrgA);
        var cash = await s.Payments.CreateAsync(orderId, Pay("Cash", 5m));
        await Assert.ThrowsAsync<InvalidOperationException>(() => s.Items.UpdateAsync(itemId, new UpdateOrderItemRequest { Quantity = 3m, DiscountAmount = 0m }));

        await s.Payments.UpdateStatusAsync((int)cash.PaymentId, new UpdatePaymentStatusRequest { Status = "Refunded" });

        Assert.NotNull(await s.Items.UpdateAsync(itemId, new UpdateOrderItemRequest { Quantity = 3m, DiscountAmount = 0m }));
    }

    [Fact]
    public async Task The_order_discount_cannot_change_after_a_Completed_payment()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, "Preparing", (5m, "Completed", "Cash", null));
        await using var s = ServicesFor(_world.OrgA);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => s.Orders.UpdateAsync(orderId, new UpdateOrderRequest { Notes = "x", DiscountAmount = 5m }));
        Assert.Equal(OrderStatusRules.DiscountHasPaymentsMessage, ex.Message);

        // notes alone (same discount) are still editable
        var ok = await s.Orders.UpdateAsync(orderId, new UpdateOrderRequest { Notes = "table by the window", DiscountAmount = 0m });
        Assert.Equal("table by the window", ok!.Notes);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(19.99)]
    public async Task An_order_cannot_be_completed_until_Completed_payments_cover_the_total(double paid)
    {
        var payments = paid == 0 ? Array.Empty<(decimal, string, string, string?)>() : new (decimal, string, string, string?)[] { ((decimal)paid, "Completed", "Cash", null) };
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, "Served", payments);
        await using var s = ServicesFor(_world.OrgA);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => s.Orders.UpdateStatusAsync(orderId, new UpdateOrderStatusRequest { Status = "Completed" }));

        Assert.Equal(OrderStatusRules.CompleteUnpaidMessage(20m - (decimal)paid), ex.Message);
        await using var check = _db.CreateContext(_world.OrgA.OrganizationId);
        var stored = await check.Orders.SingleAsync(o => o.OrderId == orderId);
        Assert.Equal("Served", stored.Status);
        Assert.Null(stored.ClosedAtUtc);
    }

    [Fact]
    public async Task A_Pending_payment_does_not_count_towards_completing_the_order()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, "Served", (20m, "Pending", "ShamCash", "TX-B1"));
        await using var s = ServicesFor(_world.OrgA);

        await Assert.ThrowsAsync<InvalidOperationException>(() => s.Orders.UpdateStatusAsync(orderId, new UpdateOrderStatusRequest { Status = "Completed" }));
    }

    [Fact]
    public async Task An_order_can_be_completed_once_Completed_payments_cover_the_total()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, "Served", (12m, "Completed", "Cash", null), (8m, "Completed", "ShamCash", "TX-C1"));
        await using var s = ServicesFor(_world.OrgA);

        var result = await s.Orders.UpdateStatusAsync(orderId, new UpdateOrderStatusRequest { Status = "Completed" });

        Assert.Equal("Completed", result!.Status);
        Assert.NotNull(result.ClosedAtUtc);
    }

    [Fact]
    public async Task A_zero_total_order_can_be_completed_without_payments()
    {
        var orderId = await SeedZeroTotalOrderAsync();
        await using var s = ServicesFor(_world.OrgA);

        var result = await s.Orders.UpdateStatusAsync(orderId, new UpdateOrderStatusRequest { Status = "Completed" });

        Assert.Equal("Completed", result!.Status);
    }

    [Fact]
    public async Task Canceling_an_order_does_not_need_payments()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, "Served");
        await using var s = ServicesFor(_world.OrgA);

        var result = await s.Orders.UpdateStatusAsync(orderId, new UpdateOrderStatusRequest { Status = "Canceled" });

        Assert.Equal("Canceled", result!.Status);
    }

    // ---------------------------------------------------------------- PaidAmount / BalanceDue

    [Fact]
    public async Task Order_details_and_list_report_PaidAmount_and_BalanceDue_from_Completed_payments_only()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA, "Pending",
            (6m, "Completed", "Cash", null), (4m, "Pending", "ShamCash", "TX-D1"), (3m, "Failed", "ShamCash", "TX-D2"), (2m, "Refunded", "Cash", null));
        await using var s = ServicesFor(_world.OrgA);

        var details = await s.Orders.GetByIdAsync(orderId);
        var listed = (await s.Orders.GetAllAsync()).Single(o => o.OrderId == orderId);

        Assert.Equal(6m, details!.PaidAmount);
        Assert.Equal(14m, details.BalanceDue);
        Assert.Equal(20m, details.TotalAmount);
        Assert.Equal(6m, listed.PaidAmount);
    }

    [Fact]
    public async Task An_unpaid_order_reports_zero_paid_and_the_full_balance()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);

        var details = await s.Orders.GetByIdAsync(orderId);

        Assert.Equal(0m, details!.PaidAmount);
        Assert.Equal(20m, details.BalanceDue);
    }

    [Fact]
    public async Task Paying_through_the_service_moves_PaidAmount_and_BalanceDue()
    {
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);

        await s.Payments.CreateAsync(orderId, Pay("Cash", 7.5m));
        var wallet = await s.Payments.CreateAsync(orderId, Pay("MTNCash", 12.5m, "TX-E1"));
        Assert.Equal(7.5m, (await s.Orders.GetByIdAsync(orderId))!.PaidAmount); // pending does not count yet

        await s.Payments.UpdateStatusAsync((int)wallet.PaymentId, new UpdatePaymentStatusRequest { Status = "Completed" });

        var details = await s.Orders.GetByIdAsync(orderId);
        Assert.Equal(20m, details!.PaidAmount);
        Assert.Equal(0m, details.BalanceDue);
    }

    // ---------------------------------------------------------------- cross-tenant

    [Fact]
    public async Task Another_organizations_order_and_payments_are_invisible_and_unchangeable()
    {
        var (foreignOrderId, _) = await SeedOrderAsync(_world.OrgB, BranchB, "Pending", (5m, "Pending", "ShamCash", "TX-F1"));
        int foreignPaymentId;
        await using (var ctxB = _db.CreateContext(_world.OrgB.OrganizationId))
            foreignPaymentId = await ctxB.Payments.Where(p => p.OrderId == foreignOrderId).Select(p => p.PaymentId).SingleAsync();
        await using var s = ServicesFor(_world.OrgA);

        // read
        Assert.Null(await s.Payments.GetByIdAsync(foreignPaymentId));
        Assert.Empty(await s.Payments.GetAllByOrderAsync(foreignOrderId));
        Assert.Null(await s.Orders.GetByIdAsync(foreignOrderId));

        // create
        await Assert.ThrowsAsync<KeyNotFoundException>(() => s.Payments.CreateAsync(foreignOrderId, Pay("Cash", 5m)));

        // change status
        Assert.Null(await s.Payments.UpdateStatusAsync(foreignPaymentId, new UpdatePaymentStatusRequest { Status = "Completed" }));

        var stored = await ReloadPaymentAsync(_world.OrgB, foreignPaymentId);
        Assert.Equal("Pending", stored.Status);
        Assert.Null(stored.ConfirmedByOrganizationUserId);
        Assert.Equal(1, await PaymentCountAsync(_world.OrgB, foreignOrderId));
    }

    [Fact]
    public async Task A_duplicate_reference_in_another_organization_is_never_revealed_by_an_error()
    {
        // B holds TX-G1 and a closed order; A recording the same reference on A's own order succeeds silently.
        await SeedOrderAsync(_world.OrgB, BranchB, "Completed", (5m, "Completed", "ShamCash", "TX-G1"));
        var (orderId, _) = await SeedOrderAsync(_world.OrgA, BranchA);
        await using var s = ServicesFor(_world.OrgA);

        var payment = await s.Payments.CreateAsync(orderId, Pay("ShamCash", 5m, "TX-G1"));

        Assert.Equal("Pending", payment.Status);
    }

    // ---------------------------------------------------------------- the rule table

    [Theory]
    [InlineData("Pending", "Completed", true)]
    [InlineData("Pending", "Failed", true)]
    [InlineData("Completed", "Refunded", true)]
    [InlineData("Pending", "Refunded", false)]
    [InlineData("Completed", "Failed", false)]
    [InlineData("Failed", "Completed", false)]
    [InlineData("Refunded", "Completed", false)]
    [InlineData("Bogus", "Completed", false)]
    public void CanMove_matches_the_documented_rules(string from, string to, bool expected)
        => Assert.Equal(expected, PaymentRules.CanMove(from, to));
}
