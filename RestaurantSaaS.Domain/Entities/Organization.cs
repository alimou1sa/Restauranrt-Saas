using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class Organization
{
    public int OrganizationId { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<OrganizationUser> OrganizationUsers { get; set; } = new List<OrganizationUser>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
