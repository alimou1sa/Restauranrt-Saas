using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.RolePermissions.RolePermissionsRequest
{
   


    public class AssignPermissionRequest
    {
        [Required]
        public int PermissionId { get; set; }
    }
}
