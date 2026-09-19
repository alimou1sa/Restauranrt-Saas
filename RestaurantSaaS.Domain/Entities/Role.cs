using RestaurantSaaS.Application.Common;
using RestaurantSaaS.Domain.Entities;
using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class Role: ITenantEntity
{
    public int RoleId { get; set; }

    public int OrganizationId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int? SystemRoleId { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual SystemRole SystemRole { get; set; } = null!;
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();


}
