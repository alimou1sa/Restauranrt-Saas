using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.Inventories.InventoriesResponse
{

    public class InventoryResponse
    {
        public int InventoryId { get; set; }

        public int BranchId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = null!;

        public decimal Quantity { get; set; }

        public decimal ReorderLevel { get; set; }

        public bool IsLowStock { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
    }
}
