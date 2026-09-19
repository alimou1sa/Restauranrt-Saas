using RestaurantSaaS.Application.Common;
using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class Customer: ITenantEntity
{
    public int CustomerId { get; set; }

    public int OrganizationId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual Organization Organization { get; set; } = null!;
}
