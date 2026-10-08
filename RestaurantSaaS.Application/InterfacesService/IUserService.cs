using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Users.UserRequest;
    using global::RestaurantSaaS.Application.DTOs.Users.UserResponse;
using RestaurantSaaS.Application.DTOs.Users;

namespace RestaurantSaaS.Application.InterfacesService
{

    public interface IUserService
    {
        Task<UserResponse> CreateAsync(CreateUserRequest request);

        Task<UserResponse?> GetMyProfileAsync();

        Task<List<UserResponse>> GetAllAsync();

        Task<UserResponse?> UpdateMyProfileAsync(UpdateUserRequest request);

        Task<bool> ChangePasswordAsync(ChangePasswordRequest request);

        Task<bool> DeleteAsync(int id);


    }
}
