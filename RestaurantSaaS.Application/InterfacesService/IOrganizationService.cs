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

        Task<OrganizationResponse?> GetAsync();

        Task<List<OrganizationResponse>> GetAllAsync();

        Task<OrganizationResponse?> UpdateAsync(UpdateOrganizationRequest request);

        Task<bool> DeleteAsync();
    }
}

