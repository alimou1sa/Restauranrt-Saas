using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.RolePermissions.RolePermissionsResponse
{

 
    public class RolePermissionResponse
    {
        public int RolePermissionId { get; set; }

        public int RoleId { get; set; }

        public int PermissionId { get; set; }

        public string PermissionCode { get; set; } = null!;

        public string PermissionName { get; set; } = null!;
    }
}
