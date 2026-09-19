using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.Inventories.InventoriesRequest
{

    // BranchId comes from the route/tenant context. This represents the
    // one-time initialization of a stock record for a Product at a Branch.
    // After creation, Quantity is only ever changed via InventoryTransaction
    // entries (see InventoryTransactions DTOs), never through a direct update.
    public class CreateInventoryRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ReorderLevel { get; set; }
    }
}
