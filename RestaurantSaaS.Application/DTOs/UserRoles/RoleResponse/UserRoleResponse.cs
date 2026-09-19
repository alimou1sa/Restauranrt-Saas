using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.UserRoles.RoleResponse;

public class UserRoleResponse
{
    public int UserRoleId { get; set; }

    public int OrganizationUserId { get; set; }

    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public DateTime AssignedAtUtc { get; set; }
}