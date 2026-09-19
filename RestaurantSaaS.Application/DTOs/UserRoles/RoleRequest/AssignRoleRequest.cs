using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace RestaurantSaaS.Application.DTOs.UserRoles.RoleRequest;


public class AssignRoleRequest
{
    [Required]
    public int RoleId { get; set; }
}
