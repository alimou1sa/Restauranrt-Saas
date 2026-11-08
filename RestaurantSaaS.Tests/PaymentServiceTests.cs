using RestaurantSaaS.Application.DTOs.Payments.PaymentsRequest;
using RestaurantSaaS.Application.Services;
using RestaurantSaaS.Infrastructure.Data;
using RestaurantSaaS.Tests.TestSupport;
using Xunit;

namespace RestaurantSaaS.Tests;

/// <summary>
/// Tests for <see cref="PaymentService"/>: how a payment is recorded, which methods need a reference,
/// and that a payment can never be larger than what is still owed.
/// The order used here has a total of 20.00.
/// </summary>
public sealed class PaymentServiceTests : IAsyncLifetime
{
    private readonly TestDatabase _db = new();
    private TestIds _ids = null!;
    private int _orderId;

    public async Task InitializeAsync()
    {
        await _db.CreateAsync();
        _ids = await _db.SeedAsync();
        _orderId = await _db.SeedOrderAsync(_ids); // total = 20.00
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    private (AppDbContext Context, PaymentService Payments) CreateService()
    {
        var tenant = new FakeCurrentTenant { OrganizationId = _ids.OrganizationId };
        var context = _db.CreateContext(_ids.OrganizationId);
        return (context, new PaymentService(context, tenant));
    }

    [Fact]
    public async Task A_cash_payment_is_completed_immediately()
    {
        var (context, payments) = CreateService();
        await using var _ = context;

        var payment = await payments.CreateAsync(_orderId, new CreatePaymentRequest
        {
            PaymentMethod = "Cash",
            Amount = 20m
        });

        Assert.Equal("Completed", payment.Status);
        Assert.NotNull(payment.PaidAtUtc);
    }

    [Fact]
    public async Task A_card_payment_without_a_reference_is_rejected()
    {
        var (context, payments) = CreateService();
        await using var _ = context;

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => payments.CreateAsync(_orderId, new CreatePaymentRequest
            {
                PaymentMethod = "Card",
                Amount = 20m
            }));

        Assert.Equal(PaymentRules.ReferenceRequiredMessage, error.Message);
    }

    [Fact]
    public async Task A_card_payment_with_a_reference_starts_as_pending()
    {
        var (context, payments) = CreateService();
        await using var _ = context;

        var payment = await payments.CreateAsync(_orderId, new CreatePaymentRequest
        {
            PaymentMethod = "Card",
            Amount = 20m,
            TransactionReference = "CARD-001"
        });

        Assert.Equal("Pending", payment.Status);
        Assert.Null(payment.PaidAtUtc);
    }

    [Fact]
    public async Task A_payment_cannot_be_more_than_the_remaining_balance()
    {
        var (context, payments) = CreateService();
        await using var _ = context;

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => payments.CreateAsync(_orderId, new CreatePaymentRequest
            {
                PaymentMethod = "Cash",
                Amount = 25m
            }));

        Assert.Equal(PaymentRules.ExceedsBalanceMessage(20m), error.Message);
    }
}
