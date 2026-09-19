using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class User
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public bool EmailConfirmed { get; set; }

    public DateTime? LastLoginAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public virtual ICollection<OrganizationUser> OrganizationUsers { get; set; } = new List<OrganizationUser>();
}
