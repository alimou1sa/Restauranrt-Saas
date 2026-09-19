using RestaurantSaaS.Domain.Common;
using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class RestaurantTable: IBranchOwnedEntity
{
    public int TableId { get; set; }

    public int BranchId { get; set; }

    public string TableNumber { get; set; } = null!;

    public int Capacity { get; set; }

    public Guid QRCodeToken { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
