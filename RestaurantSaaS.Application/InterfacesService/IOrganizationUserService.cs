using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.OrganizationUsers.OrganizationUserRequest;
    using global::RestaurantSaaS.Application.DTOs.OrganizationUsers.OrganizationUserResponse;

namespace RestaurantSaaS.Application.InterfacesService
{

    public interface IOrganizationUserService
    {
    
        Task<OrganizationUserResponse> CreateAsync( CreateOrganizationUserRequest request);

        Task<OrganizationUserResponse?> GetByIdAsync(int organizationUserId);

        Task<List<OrganizationUserResponse>> GetAllAsync();

        Task<OrganizationUserResponse?> UpdateAsync(int organizationUserId, UpdateOrganizationUserRequest request);

        Task<bool> DeleteAsync(int organizationUserId);

        Task<OrganizationUserResponse> AddMemberWithRolesAsync(AddMemberWithRolesRequest request);
    }
}
