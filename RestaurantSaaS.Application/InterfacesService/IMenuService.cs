using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Menus.MenusRequest;
    using global::RestaurantSaaS.Application.DTOs.Menus.MenusRespose;
namespace RestaurantSaaS.Application.InterfacesService
{

    public interface IMenuService
    {

        Task<MenuResponse> CreateAsync(int branchId, CreateMenuRequest request);

        Task<MenuResponse?> GetByIdAsync(int menuId);

        Task<List<MenuResponse>> GetAllByBranchAsync(int branchId);

        Task<MenuResponse?> UpdateAsync(int menuId, UpdateMenuRequest request);

        Task<bool> DeleteAsync(int menuId);
    }
}
