using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.UserRoles.RoleRequest;
    using global::RestaurantSaaS.Application.DTOs.UserRoles.RoleResponse;
namespace RestaurantSaaS.Application.InterfacesService
{

    public interface IUserRoleService
    {

        Task<UserRoleResponse> AssignAsync(int organizationUserId, AssignRoleRequest request);

        Task<List<UserRoleResponse>> GetAllByOrganizationUserAsync(int organizationUserId);

        Task<bool> RemoveAsync(int userRoleId);
    }
}
