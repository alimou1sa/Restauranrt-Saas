using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.RolePermissions.RolePermissionsRequest;
    using global::RestaurantSaaS.Application.DTOs.RolePermissions.RolePermissionsResponse;
namespace RestaurantSaaS.Application.InterfacesService
{


    public interface IRolePermissionService
    {

        Task<RolePermissionResponse> AssignAsync(int roleId, AssignPermissionRequest request);

        Task<List<RolePermissionResponse>> GetAllByRoleAsync(int roleId);

        Task<bool> RemoveAsync(int rolePermissionId);
    }
}
