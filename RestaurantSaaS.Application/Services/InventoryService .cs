using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Inventories.InventoriesRequest;
    using global::RestaurantSaaS.Application.DTOs.Inventories.InventoriesResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
    using global::RestaurantSaaS.Infrastructure;
    using Microsoft.EntityFrameworkCore;
namespace RestaurantSaaS.Application.Services
{

    public class InventoryService : IInventoryService
    {
        private readonly IAppDbContext _context;

        public InventoryService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryResponse> CreateAsync(int branchId, CreateInventoryRequest request)
        {
            var branchExists = await _context.Branches.AnyAsync(b => b.BranchId == branchId);
            if (!branchExists)
                throw new KeyNotFoundException($"Branch {branchId} was not found.");

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == request.ProductId);

            if (product is null)
                throw new KeyNotFoundException($"Product {request.ProductId} was not found.");

            var alreadyExists = await _context.Inventories
                .AnyAsync(i => i.BranchId == branchId && i.ProductId == request.ProductId);

            if (alreadyExists)
                throw new InvalidOperationException("A stock record for this product already exists at this branch.");

            var inventory = new Inventory
            {
                BranchId = branchId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                ReorderLevel = request.ReorderLevel,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            return ToResponse(inventory, product.Name);
        }

        public async Task<InventoryResponse?> GetByIdAsync(int inventoryId)
        {
            return await Projection()
                .FirstOrDefaultAsync(i => i.InventoryId == inventoryId);
        }


        public async Task<List<InventoryResponse>> GetAllByBranchAsync(int branchId)
        {
            return await Projection()
                .Where(i => i.BranchId == branchId)
                .ToListAsync();
        }

        public async Task<InventoryResponse?> UpdateSettingsAsync(int inventoryId, UpdateInventorySettingsRequest request)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.InventoryId == inventoryId);

            if (inventory is null)
                return null;

            inventory.ReorderLevel = request.ReorderLevel;
            inventory.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ToResponse(inventory, inventory.Product.Name);
        }

        public async Task<bool> DeleteAsync(int inventoryId)
        {
            var inventory = await _context.Inventories.FindAsync(inventoryId);
            if (inventory is null)
                return false;

            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();

            return true;
        }

        private IQueryable<InventoryResponse> Projection()
        {
            return _context.Inventories
                .AsNoTracking()
                .Select(i => new InventoryResponse
                {
                    InventoryId = i.InventoryId,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    ReorderLevel = i.ReorderLevel,
                    IsLowStock = i.Quantity <= i.ReorderLevel,
                    UpdatedAtUtc = i.UpdatedAtUtc
                });
        }

        private static InventoryResponse ToResponse(Inventory inventory, string productName)
        {
            return new InventoryResponse
            {
                InventoryId = inventory.InventoryId,
                ProductId = inventory.ProductId,
                ProductName = productName,
                Quantity = inventory.Quantity,
                ReorderLevel = inventory.ReorderLevel,
                IsLowStock = inventory.Quantity <= inventory.ReorderLevel,
                UpdatedAtUtc = inventory.UpdatedAtUtc
            };
        }
    }
}
