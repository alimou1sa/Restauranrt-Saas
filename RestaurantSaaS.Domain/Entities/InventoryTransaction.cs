using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class InventoryTransaction
{
    public int InventoryTransactionId { get; set; }

    public int InventoryId { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal Quantity { get; set; }

    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual Inventory Inventory { get; set; } = null!;
}
