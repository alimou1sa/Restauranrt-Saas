using RestaurantSaaS.Application.Services;
using Xunit;

namespace RestaurantSaaS.Tests;

/// <summary>
/// Tests for the order status rules. These are pure functions, so no database is needed.
/// </summary>
public class OrderStatusRulesTests
{
    [Fact]
    public void Completed_and_canceled_orders_are_closed()
    {
        Assert.True(OrderStatusRules.IsClosed("Completed"));
        Assert.True(OrderStatusRules.IsClosed("Canceled"));
        Assert.False(OrderStatusRules.IsClosed("Pending"));
    }

    [Fact]
    public void Status_moves_forward_one_step_at_a_time()
    {
        Assert.True(OrderStatusRules.CanMove("Pending", "Confirmed"));
        Assert.True(OrderStatusRules.CanMove("Confirmed", "Preparing"));
        Assert.False(OrderStatusRules.CanMove("Preparing", "Pending"));
        Assert.False(OrderStatusRules.CanMove("Completed", "Served"));
    }

    [Fact]
    public void Any_open_order_can_be_canceled()
    {
        Assert.True(OrderStatusRules.CanMove("Pending", "Canceled"));
        Assert.True(OrderStatusRules.CanMove("Served", "Canceled"));
        Assert.False(OrderStatusRules.CanMove("Completed", "Canceled"));
    }

    [Fact]
    public void Complete_unpaid_message_shows_the_balance_with_two_decimals()
    {
        var message = OrderStatusRules.CompleteUnpaidMessage(5m);

        Assert.Contains("5.00", message);
    }
}
