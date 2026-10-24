    using global::RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsRequest;
    using global::RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
  
    using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RestaurantSaaS.Application.Services
{


    public class OrderItemService : IOrderItemService
    {
        private readonly IAppDbContext _context;

        public OrderItemService(IAppDbContext context)
        {
            _context = context;
        }


        public async Task<OrderItemResponse> AddAsync(int orderId, CreateOrderItemRequest request)
        {
            var orderStatus = await _context.Orders
                .Where(o => o.OrderId == orderId)
                .Select(o => o.Status)
                .FirstOrDefaultAsync();
            if (orderStatus is null)
                throw new KeyNotFoundException($"Order {orderId} was not found.");

            if (OrderStatusRules.IsClosed(orderStatus))
                throw new InvalidOperationException(OrderStatusRules.ClosedOrderItemsMessage);

            await EnsureNoCompletedPaymentsAsync(orderId);

            var product = await _context.Products
                .Where(p => p.ProductId == request.ProductId)
                .Select(p => new { p.ProductId, p.Name, p.Price, BranchId = p.Category.Menu.BranchId })
                .FirstOrDefaultAsync();

            if (product is null)
                throw new KeyNotFoundException($"Product {request.ProductId} was not found.");

            var orderBranchId = await _context.Orders
                .Where(o => o.OrderId == orderId)
                .Select(o => o.BranchId)
                .FirstAsync();

            if (product.BranchId != orderBranchId)
                throw new InvalidOperationException($"Product {request.ProductId} does not belong to this order's branch.");

            await using var transaction = await _context.BeginTransactionAsync();
            try
            {
                var orderItem = new OrderItem
                {
                    OrderId = orderId,
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = request.Quantity,
                    DiscountAmount = request.DiscountAmount
                };

                _context.OrderItems.Add(orderItem);
                await _context.SaveChangesAsync();

                await RecalculateOrderTotalsInternalAsync(orderId);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ToResponse(orderItem);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> RemoveAsync(int orderItemId)
        {
            var orderItem = await _context.OrderItems.FindAsync(orderItemId);
            if (orderItem is null)
                return false;

            await EnsureOrderIsOpenAsync(orderItem.OrderId);

            var orderId = orderItem.OrderId;

            await using var transaction = await _context.BeginTransactionAsync();
            try
            {
                _context.OrderItems.Remove(orderItem);
                await _context.SaveChangesAsync();

                await RecalculateOrderTotalsInternalAsync(orderId);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task RecalculateOrderTotalsInternalAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order is null)
                return;

            var subTotal = await _context.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .SumAsync(oi => (decimal?)oi.LineTotal) ?? 0m;

            order.SubTotal = subTotal;
            order.TotalAmount = subTotal - order.DiscountAmount + order.TaxAmount;
            order.UpdatedAtUtc = DateTime.UtcNow;


        }

        public async Task<List<OrderItemResponse>> GetAllByOrderAsync(int orderId)
        {
            return await _context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.OrderId == orderId)
                .Select(oi => new OrderItemResponse
                {
                    OrderItemId = oi.OrderItemId,
                    ProductId = oi.ProductId,
                    ProductName = oi.ProductName,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    DiscountAmount = oi.DiscountAmount,
                    LineTotal = oi.LineTotal
                })
                .ToListAsync();
        }

        public async Task<OrderItemResponse?> UpdateAsync(int orderItemId, UpdateOrderItemRequest request)
        {
            // Look up the order status before opening the transaction: a closed order must not be touched at all.
            var parent = await _context.OrderItems
                .Where(oi => oi.OrderItemId == orderItemId)
                .Select(oi => new { oi.OrderId, oi.Order.Status })
                .FirstOrDefaultAsync();
            if (parent is null)
                return null;

            if (OrderStatusRules.IsClosed(parent.Status))
                throw new InvalidOperationException(OrderStatusRules.ClosedOrderItemsMessage);

            await EnsureNoCompletedPaymentsAsync(parent.OrderId);

            await using var transaction = await _context.BeginTransactionAsync();

            try
            {
                var orderItem = await _context.OrderItems
                    .FirstOrDefaultAsync(oi => oi.OrderItemId == orderItemId);

                if (orderItem is null)
                    return null;

                orderItem.Quantity = request.Quantity;
                orderItem.DiscountAmount = request.DiscountAmount;

                await _context.SaveChangesAsync();
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == orderItem.OrderId);

                if (order is null)
                    return null;

                var subTotal = await _context.OrderItems
                    .Where(oi => oi.OrderId == orderItem.OrderId)
                    .SumAsync(oi => (decimal?)oi.LineTotal) ?? 0m;

                order.SubTotal = subTotal;
                order.TotalAmount = subTotal - order.DiscountAmount + order.TaxAmount;
                order.UpdatedAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return ToResponse(orderItem);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task EnsureOrderIsOpenAsync(int orderId)
        {
            var status = await _context.Orders
                .Where(o => o.OrderId == orderId)
                .Select(o => o.Status)
                .FirstOrDefaultAsync();

            if (status is not null && OrderStatusRules.IsClosed(status))
                throw new InvalidOperationException(OrderStatusRules.ClosedOrderItemsMessage);

            await EnsureNoCompletedPaymentsAsync(orderId);
        }

        // Once money was received the lines are frozen; a refund (status Refunded) lifts the restriction again.
        private async Task EnsureNoCompletedPaymentsAsync(int orderId)
        {
            if (await _context.Payments.AnyAsync(p => p.OrderId == orderId && p.Status == PaymentRules.Completed))
                throw new InvalidOperationException(OrderStatusRules.ItemsHavePaymentsMessage);
        }

        private static OrderItemResponse ToResponse(OrderItem orderItem)
        {
            return new OrderItemResponse
            {
                OrderItemId = orderItem.OrderItemId,
                ProductId = orderItem.ProductId,
                ProductName = orderItem.ProductName,
                UnitPrice = orderItem.UnitPrice,
                Quantity = orderItem.Quantity,
                DiscountAmount = orderItem.DiscountAmount,
                LineTotal = orderItem.LineTotal
            };
        }
    }
}
