using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.RestaurantTables.RestaurantTablesRequest;
    using global::RestaurantSaaS.Application.DTOs.RestaurantTables.RestaurantTablesResponse;
namespace RestaurantSaaS.Application.InterfacesService
{

    public interface IRestaurantTableService
    {
        Task<RestaurantTableResponse> CreateAsync(int branchId, CreateRestaurantTableRequest request);

        Task<RestaurantTableResponse?> GetByIdAsync(int tableId);

        Task<List<RestaurantTableResponse>> GetAllByBranchAsync(int branchId);

        Task<RestaurantTableResponse?> UpdateAsync(int tableId, UpdateRestaurantTableRequest request);

        Task<bool> DeleteAsync(int tableId);
    }
}
