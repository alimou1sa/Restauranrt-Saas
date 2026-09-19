using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace RestaurantSaaS.Application.DTOs.Roles.RoleRequest;

public class CreateCustomRoleRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(300)]
    public string? Description { get; set; }
}


