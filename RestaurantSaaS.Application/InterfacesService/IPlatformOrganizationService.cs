using RestaurantSaaS.Application.DTOs.Organizations.OrganizationsRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.InterfacesService
{
    public interface IPlatformOrganizationService
    {
        Task<List<OrganizationResponse>> GetAllAsync();
        Task<OrganizationResponse?> GetByIdAsync(int organizationId);
        Task<OrganizationResponse?> SetActiveAsync(int organizationId, bool isActive);
    }
}
