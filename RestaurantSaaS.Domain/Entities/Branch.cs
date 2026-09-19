using RestaurantSaaS.Application.Common;
using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class Branch: ITenantEntity
{
    public int BranchId { get; set; }

    public int OrganizationId { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Organization Organization { get; set; } = null!;

    public virtual ICollection<OrganizationUser> OrganizationUsers { get; set; } = new List<OrganizationUser>();

    public virtual ICollection<RestaurantTable> RestaurantTables { get; set; } = new List<RestaurantTable>();
}
