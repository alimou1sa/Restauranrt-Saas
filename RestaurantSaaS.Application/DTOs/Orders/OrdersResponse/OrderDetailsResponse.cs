using System;
using System.Collections.Generic;
using RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsResponse;

namespace RestaurantSaaS.Application.DTOs.Orders.OrdersResponse;

public class OrderDetailsResponse
{
    public int OrderId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public int BranchId { get; set; }

    public string BranchName { get; set; } = null!;

    public int? TableId { get; set; }

    public string? TableName { get; set; }

    public int? CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public string Status { get; set; } = null!;

    public string OrderType { get; set; } = null!;

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public DateTime? ClosedAtUtc { get; set; }

    public List<OrderItemResponse> Items { get; set; } = new();
}