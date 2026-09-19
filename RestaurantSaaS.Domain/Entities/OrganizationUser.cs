using RestaurantSaaS.Application.Common;
using System;
using System.Collections.Generic;


namespace RestaurantSaaS.Infrastructure;

public partial class OrganizationUser: ITenantEntity
{
    public int OrganizationUserId { get; set; }

    public int OrganizationId { get; set; }

    public int UserId { get; set; }

    public int? BranchId { get; set; }

    public bool IsActive { get; set; }

    public DateTime JoinedAtUtc { get; set; }

    public DateTime? RemovedAtUtc { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Organization Organization { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
