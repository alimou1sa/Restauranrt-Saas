using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class Order
{
    public int OrderId { get; set; }

    public int BranchId { get; set; }

    public int? TableId { get; set; }

    public int? CustomerId { get; set; }

    public string OrderNumber { get; set; } = null!;

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

    public virtual Branch Branch { get; set; } = null!;

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual RestaurantTable? RestaurantTable { get; set; }
}
