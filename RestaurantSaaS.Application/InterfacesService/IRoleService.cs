using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Roles.RoleRequest;
    using global::RestaurantSaaS.Application.DTOs.Roles.RoleResponse;


namespace RestaurantSaaS.Application.InterfacesService
{

    public interface IRoleService
    {
        Task<RoleResponse> CreateCustomAsync(CreateCustomRoleRequest request);

        Task<RoleResponse> AddSystemRoleAsync(int systemRoleId);

        Task<RoleResponse?> GetByIdAsync(int roleId);

        Task<List<RoleResponse>> GetAllAsync();

        Task<RoleResponse?> UpdateCustomAsync(int roleId,UpdateCustomRoleRequest request);

        Task<bool> DeleteCustomAsync(int roleId);
    }
}
