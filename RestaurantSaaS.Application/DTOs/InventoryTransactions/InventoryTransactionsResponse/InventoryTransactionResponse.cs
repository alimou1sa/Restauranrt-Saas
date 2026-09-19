using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.InventoryTransactions.InventoryTransactionsResponse
{

    public class InventoryTransactionResponse
    {
        public int InventoryTransactionId { get; set; }

        public int InventoryId { get; set; }

        public string ProductName { get; set; } = null!;

        public string TransactionType { get; set; } = null!;

        public decimal Quantity { get; set; }

        public string? ReferenceType { get; set; }

        public int? ReferenceId { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
