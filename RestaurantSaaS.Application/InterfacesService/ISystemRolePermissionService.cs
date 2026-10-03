using RestaurantSaaS.Application.DTOs.SystemRolePermissions.SystemRolePermissionsRequest;
using RestaurantSaaS.Application.DTOs.SystemRolePermissions.SystemRolePermissionsResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.InterfacesService
{
    public interface ISystemRolePermissionService
    {
        Task<SystemRolePermissionResponse> AssignAsync(int systemRoleId, CreateSystemRolePermissionRequest request);
        Task<List<SystemRolePermissionResponse>> GetAllBySystemRoleAsync(int systemRoleId);
        Task<bool> RemoveAsync(int systemRolePermissionId);
    }
}
