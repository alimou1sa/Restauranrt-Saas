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

        Task<UserResponse?> GetByIdAsync(int id);

        Task<List<UserResponse>> GetAllAsync();

        Task<UserResponse?> UpdateAsync(int id,UpdateUserRequest request);

        Task<bool> ChangePasswordAsync(int id,ChangePasswordRequest request);

        Task<bool> DeleteAsync(int id);


    }
}
