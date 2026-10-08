using RestaurantSaaS.Application.DTOs.Users.UserResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.InterfacesService
{
    public interface IPlatformUserService
    {
        Task<List<UserResponse>> GetAllAsync();
        Task<UserResponse?> GetByIdAsync(int userId);
        Task<UserResponse?> SetActiveAsync(int userId, bool isActive);
    }
}
