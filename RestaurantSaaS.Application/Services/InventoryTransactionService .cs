using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.InventoryTransactions.InventoryTransactionsRequest;
    using global::RestaurantSaaS.Application.DTOs.InventoryTransactions.InventoryTransactionsResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
    using global::RestaurantSaaS.Infrastructure;
    using Microsoft.EntityFrameworkCore;

namespace RestaurantSaaS.Application.Services
{

    public class InventoryTransactionService : IInventoryTransactionService
    {
        private readonly IAppDbContext _context;

        public InventoryTransactionService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryTransactionResponse> CreateAsync(int inventoryId, CreateInventoryTransactionRequest request)
        {
              var inventory = await _context.Inventories
                  .Include(i => i.Product).FirstOrDefaultAsync(i => i.InventoryId == inventoryId);



            if (inventory is null)
                throw new KeyNotFoundException($"Inventory record {inventoryId} was not found.");

            var resultingQuantity = inventory.Quantity + request.Quantity;
            if (resultingQuantity < 0)
                throw new InvalidOperationException("This movement would result in a negative stock quantity.");

            var transaction = new InventoryTransaction
            {
                InventoryId = inventoryId,
                TransactionType = request.TransactionType,
                Quantity = request.Quantity,
                ReferenceType = request.ReferenceType,
                ReferenceId = request.ReferenceId,
                Notes = request.Notes,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.InventoryTransactions.Add(transaction);

            inventory.Quantity = resultingQuantity;
            inventory.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ToResponse(transaction, inventory.Product.Name);
        }

        public async Task<List<InventoryTransactionResponse>> GetAllByBranchAsync(int branchId)
        {
            return await _context.InventoryTransactions
                .AsNoTracking()
                .Where(t => t.Inventory.BranchId == branchId)
                .OrderByDescending(t => t.CreatedAtUtc)
                .Select(t => new InventoryTransactionResponse
                {
                    InventoryTransactionId = t.InventoryTransactionId,
                    InventoryId = t.InventoryId,
                    ProductName = t.Inventory.Product.Name,
                    TransactionType = t.TransactionType,
                    Quantity = t.Quantity,
                    ReferenceType = t.ReferenceType,
                    ReferenceId = t.ReferenceId,
                    Notes = t.Notes,
                    CreatedAtUtc = t.CreatedAtUtc
                })
                .ToListAsync();
        }

        private static InventoryTransactionResponse ToResponse(InventoryTransaction transaction, string productName)
        {
            return new InventoryTransactionResponse
            {
                InventoryTransactionId = transaction.InventoryTransactionId,
                InventoryId = transaction.InventoryId,
                ProductName = productName,
                TransactionType = transaction.TransactionType,
                Quantity = transaction.Quantity,
                ReferenceType = transaction.ReferenceType,
                ReferenceId = transaction.ReferenceId,
                Notes = transaction.Notes,
                CreatedAtUtc = transaction.CreatedAtUtc
            };
        }
    }
}
