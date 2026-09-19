using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Permissions.PermissionRequest;
    using global::RestaurantSaaS.Application.DTOs.Permissions.PermissionResponse;
namespace RestaurantSaaS.Application.InterfacesService
{




    public interface IPermissionService
    {
        Task<PermissionResponse> CreateAsync(CreatePermissionRequest request);

        Task<PermissionResponse?> GetByIdAsync(int permissionId);

        Task<List<PermissionResponse>> GetAllAsync();

        Task<PermissionResponse?> UpdateAsync(int permissionId, UpdatePermissionRequest request);

        Task<bool> DeleteAsync(int permissionId);
    }
}
