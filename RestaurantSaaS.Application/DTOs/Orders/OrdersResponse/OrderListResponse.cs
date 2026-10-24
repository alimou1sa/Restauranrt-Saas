using System;

namespace RestaurantSaaS.Application.DTOs.Orders.OrdersResponse;


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

    // Sum of Completed payments.
    public decimal PaidAmount { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}