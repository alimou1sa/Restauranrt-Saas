using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsRequest;
using global::RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsResponse;
using global::RestaurantSaaS.Application.DTOs.Orders.OrdersRequest;
using global::RestaurantSaaS.Application.DTOs.Orders.OrdersResponse;
using global::RestaurantSaaS.Application.InterfacesService;
using global::RestaurantSaaS.Infrastructure;
using Microsoft.EntityFrameworkCore;
namespace RestaurantSaaS.Application.Services
{

    public class OrderService : IOrderService
    {
        private static readonly string[] AllowedStatuses =
            { "Pending", "Confirmed", "Preparing", "Ready", "Served", "Completed", "Canceled" };

        private static readonly string[] ClosedStatuses = { "Completed", "Canceled" };

        private readonly IAppDbContext _context;

        public OrderService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderDetailsResponse> CreateAsync(int branchId, CreateOrderRequest request)
        {
            var branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.BranchId == branchId);

            if (branch is null)
                throw new KeyNotFoundException($"Branch {branchId} was not found.");

            if (request.TableId is not null)
            {
                var tableBelongsToBranch = await _context.RestaurantTables
                    .AnyAsync(t => t.TableId == request.TableId && t.BranchId == branchId);

                if (!tableBelongsToBranch)
                    throw new InvalidOperationException($"Table {request.TableId} does not belong to this branch.");
            }

            if (request.CustomerId is not null)
            {
                var customerBelongsToOrganization = await _context.Customers
                    .AnyAsync(c => c.CustomerId == request.CustomerId && c.OrganizationId == branch.OrganizationId);

                if (!customerBelongsToOrganization)
                    throw new InvalidOperationException($"Customer {request.CustomerId} does not belong to this organization.");
            }

            var orderCount = await _context.Orders.CountAsync(o => o.BranchId == branchId);
            var orderNumber = $"ORD-{branchId}-{orderCount + 1:D5}";

            var order = new Order
            {
                BranchId = branchId,
                TableId = request.TableId,
                CustomerId = request.CustomerId,
                OrderNumber = orderNumber,
                Status = "Pending",
                OrderType = request.OrderType,
                SubTotal = 0,
                DiscountAmount = 0,
                TaxAmount = 0,
                TotalAmount = 0,
                Notes = request.Notes,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(order.OrderId))!;
        }


        public async Task<OrderDetailsResponse?> GetByIdAsync(int orderId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderId == orderId)
                .Select(o => new OrderDetailsResponse
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    BranchId = o.BranchId,
                    BranchName = o.Branch.Name,
                    TableId = o.TableId,
                    TableName = o.RestaurantTable != null ? o.RestaurantTable.TableNumber : null,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer == null ? null
                        : o.Customer.LastName == null ? o.Customer.FirstName
                        : o.Customer.FirstName + " " + o.Customer.LastName,
                    Status = o.Status,
                    OrderType = o.OrderType,
                    SubTotal = o.SubTotal,
                    DiscountAmount = o.DiscountAmount,
                    TaxAmount = o.TaxAmount,
                    TotalAmount = o.TotalAmount,
                    Notes = o.Notes,
                    CreatedAtUtc = o.CreatedAtUtc,
                    UpdatedAtUtc = o.UpdatedAtUtc,
                    ClosedAtUtc = o.ClosedAtUtc,
                    Items = o.OrderItems.Select(oi => new OrderItemResponse
                    {
                        OrderItemId = oi.OrderItemId,
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        UnitPrice = oi.UnitPrice,
                        Quantity = oi.Quantity,
                        DiscountAmount = oi.DiscountAmount,
                        LineTotal = oi.LineTotal
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }


        public async Task<List<OrderListResponse>> GetAllByOrganizationAsync(int organizationId)
        {
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Branch.OrganizationId == organizationId)
                .Select(o => new OrderListResponse
                {
                    OrderId = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    BranchName = o.Branch.Name,
                    TableName = o.RestaurantTable != null ? o.RestaurantTable.TableNumber : null,
                    CustomerName = o.Customer == null ? null
                        : o.Customer.LastName == null ? o.Customer.FirstName
                        : o.Customer.FirstName + " " + o.Customer.LastName,
                    Status = o.Status,
                    OrderType = o.OrderType,
                    TotalAmount = o.TotalAmount,
                    CreatedAtUtc = o.CreatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<OrderDetailsResponse?> UpdateAsync(int orderId, UpdateOrderRequest request)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order is null)
                return null;

            if (request.TableId is not null)
            {
                var tableBelongsToBranch = await _context.RestaurantTables
                    .AnyAsync(t => t.TableId == request.TableId && t.BranchId == order.BranchId);

                if (!tableBelongsToBranch)
                    throw new InvalidOperationException($"Table {request.TableId} does not belong to this order's branch.");
            }

            if (request.CustomerId is not null)
            {
                var organizationId = await _context.Branches
                    .Where(b => b.BranchId == order.BranchId)
                    .Select(b => b.OrganizationId)
                    .FirstAsync();

                var customerBelongsToOrganization = await _context.Customers
                    .AnyAsync(c => c.CustomerId == request.CustomerId && c.OrganizationId == organizationId);

                if (!customerBelongsToOrganization)
                    throw new InvalidOperationException($"Customer {request.CustomerId} does not belong to this order's organization.");
            }

            order.TableId = request.TableId;
            order.CustomerId = request.CustomerId;
            order.Notes = request.Notes;
            order.DiscountAmount = request.DiscountAmount;
            order.TotalAmount = order.SubTotal - order.DiscountAmount + order.TaxAmount;
            order.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(orderId);
        }

        public async Task<OrderDetailsResponse?> UpdateStatusAsync(int orderId, UpdateOrderStatusRequest request)
        {
            if (!AllowedStatuses.Contains(request.Status))
                throw new InvalidOperationException($"'{request.Status}' is not a valid order status.");

            var order = await _context.Orders.FindAsync(orderId);
            if (order is null)
                return null;

            order.Status = request.Status;
            order.UpdatedAtUtc = DateTime.UtcNow;

            if (ClosedStatuses.Contains(request.Status) && order.ClosedAtUtc is null)
                order.ClosedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(orderId);
        }

        public async Task<bool> DeleteAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order is null)
                return false;

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
