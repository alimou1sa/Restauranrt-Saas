using RestaurantSaaS.Application.DTOs.Auth.Request;
using RestaurantSaaS.Application.DTOs.Auth.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.InterfacesService
{

    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<List<OrganizationOptionResponse>> GetMyOrganizationsAsync(int userId);
        Task<AuthTokenResponse> SelectOrganizationAsync(int userId, SelectOrganizationRequest request);
    }
}
