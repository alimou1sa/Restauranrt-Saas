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
        Task<RoleResponse> CreateCustomAsync(int organizationId,CreateCustomRoleRequest request);

        Task<RoleResponse> AddSystemRoleAsync(int organizationId,int systemRoleId);

        Task<RoleResponse?> GetByIdAsync(int roleId);

        Task<List<RoleResponse>> GetAllByOrganizationAsync(int organizationId);

        Task<RoleResponse?> UpdateCustomAsync(int roleId,UpdateCustomRoleRequest request);

        Task<bool> DeleteCustomAsync(int roleId);
    }
}
