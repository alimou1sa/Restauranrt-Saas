using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;

namespace RestaurantSaaS.Application.DTOs.InventoryTransactions.InventoryTransactionsRequest
{




    // InventoryId comes from the route (e.g. POST /inventory/{inventoryId}/transactions).
    // This is the only way stock quantity should change - the server applies
    // this movement to the related Inventory.Quantity as part of the same
    // business operation. There is no Update/Delete DTO: transactions are an
    // immutable audit log.
    public class CreateInventoryTransactionRequest
    {
        [Required, MaxLength(30)]
        public string TransactionType { get; set; } = null!;

        [Required]
        public decimal Quantity { get; set; }

        [MaxLength(50)]
        public string? ReferenceType { get; set; }

        public int? ReferenceId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
