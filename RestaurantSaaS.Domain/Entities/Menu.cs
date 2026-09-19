using RestaurantSaaS.Domain.Common;
using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class Menu: IBranchOwnedEntity
{
    public int MenuId { get; set; }

    public int BranchId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsPublished { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
