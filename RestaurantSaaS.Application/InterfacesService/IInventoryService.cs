using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Inventories.InventoriesRequest;
    using global::RestaurantSaaS.Application.DTOs.Inventories.InventoriesResponse;

namespace RestaurantSaaS.Application.InterfacesService
{


    public interface IInventoryService
    {

        Task<InventoryResponse> CreateAsync(int branchId, CreateInventoryRequest request);

        Task<InventoryResponse?> GetByIdAsync(int inventoryId);

        Task<List<InventoryResponse>> GetAllByBranchAsync(int branchId);

        Task<InventoryResponse?> UpdateSettingsAsync(int inventoryId, UpdateInventorySettingsRequest request);

        Task<bool> DeleteAsync(int inventoryId);
    }
}
