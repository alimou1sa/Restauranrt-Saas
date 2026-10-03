using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.SystemRolePermissions.SystemRolePermissionsResponse
{
    public class SystemRolePermissionResponse
    {
        public int SystemRolePermissionId { get; set; }
        public int SystemRoleId { get; set; }
        public string SystemRoleName { get; set; } = null!;
        public int PermissionId { get; set; }
        public string PermissionCode { get; set; } = null!;
        public string PermissionName { get; set; } = null!;
    }
}
