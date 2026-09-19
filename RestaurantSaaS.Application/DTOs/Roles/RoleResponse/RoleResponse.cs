using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.Roles.RoleResponse;

public class RoleResponse
{
    public int RoleId { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int? SystemRoleId { get; set; }
    public string? SystemRoleName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

