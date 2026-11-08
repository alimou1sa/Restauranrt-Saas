using RestaurantSaaS.Application.Services;
using Xunit;

namespace RestaurantSaaS.Tests;

/// <summary>
/// Tests for <see cref="PaymentRules"/>. These are pure functions,
/// so no database and no TestSupport are needed.
/// </summary>
public class PaymentRulesTests
{
    [Fact]
    public void Cash_is_an_allowed_payment_method()
    {
        Assert.Contains("Cash", PaymentRules.AllowedMethods);
    }

    [Fact]
    public void A_pending_payment_can_become_completed_or_failed()
    {
        Assert.True(PaymentRules.CanMove(PaymentRules.Pending, PaymentRules.Completed));
        Assert.True(PaymentRules.CanMove(PaymentRules.Pending, PaymentRules.Failed));
        Assert.False(PaymentRules.CanMove(PaymentRules.Pending, PaymentRules.Refunded));
    }

    [Fact]
    public void Only_a_completed_payment_can_be_refunded()
    {
        Assert.True(PaymentRules.CanMove(PaymentRules.Completed, PaymentRules.Refunded));
        Assert.False(PaymentRules.CanMove(PaymentRules.Failed, PaymentRules.Refunded));
    }

    [Fact]
    public void A_completed_payment_cannot_go_back_to_pending()
    {
        Assert.False(PaymentRules.CanMove(PaymentRules.Completed, PaymentRules.Pending));
    }

    [Fact]
    public void The_balance_message_uses_two_decimals()
    {
        var message = PaymentRules.ExceedsBalanceMessage(7.5m);

        Assert.Equal("The payment exceeds the remaining balance of 7.50.", message);
    }
}
