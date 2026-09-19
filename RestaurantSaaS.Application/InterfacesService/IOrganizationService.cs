using RestaurantSaaS.Application.DTOs.Organizations.OrganizationsRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
  using RestaurantSaaS.Application.DTOs.Organizations;



namespace RestaurantSaaS.Application.InterfacesService
{

    public interface IOrganizationService
    {
        Task<OrganizationResponse> CreateAsync(CreateOrganizationRequest request);

        Task<OrganizationResponse?> GetByIdAsync(int id);

        Task<List<OrganizationResponse>> GetAllAsync();

        Task<OrganizationResponse?> UpdateAsync(int id,UpdateOrganizationRequest request);

        Task<bool> DeleteAsync(int id);
    }
}

