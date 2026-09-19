using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public decimal Quantity { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal LineTotal { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
