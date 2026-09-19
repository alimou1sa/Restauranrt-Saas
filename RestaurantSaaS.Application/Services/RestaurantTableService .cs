using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.RestaurantTables.RestaurantTablesRequest;
    using global::RestaurantSaaS.Application.DTOs.RestaurantTables.RestaurantTablesResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
    using global::RestaurantSaaS.Infrastructure;
    using Microsoft.EntityFrameworkCore;
namespace RestaurantSaaS.Application.Services
{


    public class RestaurantTableService : IRestaurantTableService
    {
        private readonly IAppDbContext _context;

        public RestaurantTableService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<RestaurantTableResponse> CreateAsync(int branchId, CreateRestaurantTableRequest request)
        {
            var branchExists = await _context.Branches
                .AnyAsync(b => b.BranchId == branchId);

            if (!branchExists)
                throw new KeyNotFoundException($"Branch {branchId} was not found.");

            var numberExists = await _context.RestaurantTables
                .AnyAsync(t => t.BranchId == branchId && t.TableNumber == request.TableNumber);

            if (numberExists)
                throw new InvalidOperationException($"A table numbered '{request.TableNumber}' already exists in this branch.");

            var table = new RestaurantTable
            {
                BranchId = branchId,
                TableNumber = request.TableNumber,
                Capacity = request.Capacity,
                QRCodeToken = Guid.NewGuid(),
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.RestaurantTables.Add(table);
            await _context.SaveChangesAsync();

            return ToResponse(table);
        }

        public async Task<RestaurantTableResponse?> GetByIdAsync(int tableId)
        {
            var table = await _context.RestaurantTables
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TableId == tableId);

            return table is null ? null : ToResponse(table);
        }

        public async Task< List<RestaurantTableResponse>> GetAllByBranchAsync(int branchId)
        {
            var tables = await _context.RestaurantTables
                .AsNoTracking()
                .Where(t => t.BranchId == branchId)
                .ToListAsync();

            return tables.Select(ToResponse).ToList();
        }

        public async Task<RestaurantTableResponse?> UpdateAsync(int tableId, UpdateRestaurantTableRequest request)
        {
            var table = await _context.RestaurantTables.FindAsync(tableId);
            if (table is null)
                return null;

            var numberExists = await _context.RestaurantTables
                .AnyAsync(t => t.BranchId == table.BranchId
                            && t.TableNumber == request.TableNumber
                            && t.TableId != tableId);

            if (numberExists)
                throw new InvalidOperationException($"A table numbered '{request.TableNumber}' already exists in this branch.");

            table.TableNumber = request.TableNumber;
            table.Capacity = request.Capacity;
            table.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return ToResponse(table);
        }

        public async Task<bool> DeleteAsync(int tableId)
        {
            var table = await _context.RestaurantTables.FindAsync(tableId);
            if (table is null)
                return false;

            _context.RestaurantTables.Remove(table);
            await _context.SaveChangesAsync();

            return true;
        }

        private static RestaurantTableResponse ToResponse(RestaurantTable table)
        {
            return new RestaurantTableResponse
            {
                TableId = table.TableId,
                TableNumber = table.TableNumber,
                Capacity = table.Capacity,
                QRCodeToken = table.QRCodeToken,
                IsActive = table.IsActive,
                CreatedAtUtc = table.CreatedAtUtc
            };
        }
    }
}
