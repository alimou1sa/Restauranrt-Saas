using RestaurantSaaS.Application.Common;
using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class UserRole: ITenantEntity
{
    public int UserRoleId { get; set; }

    public int OrganizationUserId { get; set; }

    public int RoleId { get; set; }

    public DateTime AssignedAtUtc { get; set; }
    public int OrganizationId { get; set; }
    public virtual OrganizationUser OrganizationUser { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
