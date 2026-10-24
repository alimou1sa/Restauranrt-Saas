using global::RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsRequest;
using global::RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsResponse;
using global::RestaurantSaaS.Application.DTOs.Orders.OrdersRequest;
using global::RestaurantSaaS.Application.DTOs.Orders.OrdersResponse;
using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Domain.Common;
using RestaurantSaaS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RestaurantSaaS.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentTenant _currentTenant;
        public OrderService(IAppDbContext context, ICurrentTenant currentTenant)
        {
            _context = context;
            _currentTenant = currentTenant;
        }



        public async Task<OrderDetailsResponse> CreateAsync(int branchId, CreateOrderRequest request)
        {
            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.BranchId == branchId);

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



            var orderNumber = $"ORD-{branchId}-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..4]}";


            var order = new Order
            {
                BranchId = branchId,
                OrganizationId = branch.OrganizationId,

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
                    PaidAmount = o.Payments.Where(p => p.Status == PaymentRules.Completed).Sum(p => (decimal?)p.Amount) ?? 0m,
                    BalanceDue = o.TotalAmount - (o.Payments.Where(p => p.Status == PaymentRules.Completed).Sum(p => (decimal?)p.Amount) ?? 0m),
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


        public async Task<List<OrderListResponse>> GetAllAsync()
        {
            var organizationId = _currentTenant.OrganizationId ?? throw new UnauthorizedAccessException("No organization context.");

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
                    PaidAmount = o.Payments.Where(p => p.Status == PaymentRules.Completed).Sum(p => (decimal?)p.Amount) ?? 0m,
                    CreatedAtUtc = o.CreatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<OrderDetailsResponse?> UpdateAsync(int orderId, UpdateOrderRequest request)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order is null)
                return null;

            if (OrderStatusRules.IsClosed(order.Status))
                throw new InvalidOperationException(OrderStatusRules.ClosedOrderModifyMessage);

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

            // A discount change moves the total under money that was already received, so it is refused once a payment completed.
            if (request.DiscountAmount != order.DiscountAmount
                && await _context.Payments.AnyAsync(p => p.OrderId == orderId && p.Status == PaymentRules.Completed))
                throw new InvalidOperationException(OrderStatusRules.DiscountHasPaymentsMessage);

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
            if (!OrderStatusRules.AllStatuses.Contains(request.Status))
                throw new InvalidOperationException($"'{request.Status}' is not a valid order status.");

            var order = await _context.Orders.FindAsync(orderId);
            if (order is null)
                return null;

            await using var transaction = await _context.BeginTransactionAsync();
            try
            {
                // The UPDATE takes the order row lock (shared with PaymentService), then the status and total are read
                // again: what is checked below cannot change underneath this request until it commits.
                order.UpdatedAtUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var current = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.OrderId == orderId)
                    .Select(o => new { o.Status, o.TotalAmount })
                    .FirstAsync();

                if (OrderStatusRules.IsClosed(current.Status))
                    throw new InvalidOperationException(OrderStatusRules.ClosedOrderStatusMessage);

                if (!OrderStatusRules.CanMove(current.Status, request.Status))
                    throw new InvalidOperationException(OrderStatusRules.BackwardStatusMessage);

                if (request.Status == OrderStatusRules.Completed)
                {
                    var paid = await _context.Payments
                        .Where(p => p.OrderId == orderId && p.Status == PaymentRules.Completed)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                    // A zero-total order has nothing to pay and may be completed.
                    if (paid < current.TotalAmount)
                        throw new InvalidOperationException(OrderStatusRules.CompleteUnpaidMessage(current.TotalAmount - paid));
                }

                var now = DateTime.UtcNow;
                order.Status = request.Status;
                order.UpdatedAtUtc = now;

                // Only open orders reach this point, so ClosedAtUtc is always empty here: it is set once, when the order
                // closes, and can never change afterwards because closed orders are rejected above.
                if (OrderStatusRules.IsClosed(request.Status))
                    order.ClosedAtUtc = now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(orderId);
        }

        public async Task<bool> DeleteAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order is null)
                return false;

            if (OrderStatusRules.IsClosed(order.Status))
                throw new InvalidOperationException(OrderStatusRules.ClosedOrderDeleteMessage);

            if (order.Status != OrderStatusRules.Pending)
                throw new InvalidOperationException(OrderStatusRules.DeleteOnlyPendingMessage);

            if (await _context.Payments.AnyAsync(p => p.OrderId == orderId))
                throw new InvalidOperationException(OrderStatusRules.DeleteWithPaymentsMessage);

            // A Pending order without payments may still have items; they go with it (the FK is not cascading).
            await using var transaction = await _context.BeginTransactionAsync();
            try
            {
                var items = await _context.OrderItems.Where(oi => oi.OrderId == orderId).ToListAsync();
                _context.OrderItems.RemoveRange(items);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return true;
        }
    }
}
