using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.InventoryTransactions.InventoryTransactionsRequest;
    using global::RestaurantSaaS.Application.DTOs.InventoryTransactions.InventoryTransactionsResponse;
namespace RestaurantSaaS.Application.InterfacesService
{


    public interface IInventoryTransactionService
    {

        Task<InventoryTransactionResponse> CreateAsync(int inventoryId, CreateInventoryTransactionRequest request);

   
        Task<List<InventoryTransactionResponse>> GetAllByBranchAsync(int branchId);
    }
}
