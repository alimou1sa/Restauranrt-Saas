using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Customers.CustomersRequest;
    using global::RestaurantSaaS.Application.DTOs.Customers.CustomersResponse;
namespace RestaurantSaaS.Application.InterfacesService
{

    public interface ICustomerService
    {
        Task<CustomerDetailsResponse> CreateAsync(int organizationId, CreateCustomerRequest request);

        Task<CustomerDetailsResponse?> GetByIdAsync(int customerId);

        Task<List<CustomerListResponse>> GetAllByOrganizationAsync(int organizationId);

        Task<CustomerDetailsResponse?> UpdateAsync(int customerId, UpdateCustomerRequest request);

        Task<bool> DeleteAsync(int customerId);
    }
}
