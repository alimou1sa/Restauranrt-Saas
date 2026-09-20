
using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class Inventory
{
    public int InventoryId { get; set; }

    public int BranchId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    public decimal ReorderLevel { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    public virtual Product Product { get; set; } = null!;
}
