using System;

namespace RestaurantSaaS.Application.DTOs.Orders.OrdersResponse;

// Real-world assumption change from the previous version: an "Orders" list
// is typically viewed across an entire Organization (a manager overseeing
// several branches wants one consolidated view), not one branch at a time.
// That is why BranchName is now included - it answers "which branch is
// this order from?", a question that only matters once you're looking
// across branches. TableName/CustomerName replace TableId/CustomerId for
// the same reason: a list row needs to say who/where, not raw ids.
// Raw ids (BranchId/TableId/CustomerId) are intentionally left out of the
// list - a table row doesn't act on them directly; OrderDetailsResponse
// carries them for the edit/detail screen where they're actually needed.
public class OrderListResponse
{
    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public string BranchName { get; set; } = null!;

    public string? TableName { get; set; }

    public string? CustomerName { get; set; }

    public string Status { get; set; } = null!;

    public string OrderType { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}