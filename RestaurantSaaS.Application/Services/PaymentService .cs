    using global::RestaurantSaaS.Application.DTOs.Payments;
    using global::RestaurantSaaS.Application.DTOs.Payments.PaymentsRequest;
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

    public class PaymentService : IPaymentService
    {
        private readonly IAppDbContext _context;
        private readonly ICurrentTenant _currentTenant;

        public PaymentService(IAppDbContext context, ICurrentTenant currentTenant)
        {
            _context = context;
            _currentTenant = currentTenant;
        }

        public async Task<PaymentResponse> CreateAsync(int orderId, CreatePaymentRequest request)
        {
            var orderExists = await _context.Orders
                .AnyAsync(o => o.OrderId == orderId);

            if (!orderExists)
                throw new KeyNotFoundException($"Order {orderId} was not found.");

            if (!PaymentRules.AllowedMethods.Contains(request.PaymentMethod))
                throw new InvalidOperationException($"'{request.PaymentMethod}' is not a valid payment method.");

            if (request.Amount <= 0)
                throw new InvalidOperationException(PaymentRules.AmountMustBePositiveMessage);

            // The column is decimal(18, 2): reject extra decimals instead of letting the database round them silently.
            if (decimal.Round(request.Amount, 2) != request.Amount)
                throw new InvalidOperationException(PaymentRules.AmountDecimalsMessage);

            var isCash = request.PaymentMethod == PaymentRules.Cash;
            var reference = string.IsNullOrWhiteSpace(request.TransactionReference) ? null : request.TransactionReference.Trim();

            if (!isCash && reference is null)
                throw new InvalidOperationException(PaymentRules.ReferenceRequiredMessage);

            await using var transaction = await _context.BeginTransactionAsync();
            try
            {
                // Taking the order row's update lock makes concurrent payments on the same order wait for each other,
                // so the balance check below cannot be raced by a second request.
                var order = await LockOrderAsync(orderId)
                    ?? throw new KeyNotFoundException($"Order {orderId} was not found.");

                if (order.Status == OrderStatusRules.Canceled)
                    throw new InvalidOperationException(PaymentRules.OrderCanceledMessage);

                if (!isCash)
                {
                    // A failed payment never moved money, so its reference may be recorded again.
                    var duplicate = await _context.Payments.AnyAsync(p =>
                        p.Order.OrganizationId == order.OrganizationId
                        && p.PaymentMethod == request.PaymentMethod
                        && p.TransactionReference == reference
                        && p.Status != PaymentRules.Failed);

                    if (duplicate)
                        throw new InvalidOperationException(PaymentRules.ReferenceDuplicateMessage);
                }

                var committed = await _context.Payments
                    .Where(p => p.OrderId == orderId && (p.Status == PaymentRules.Completed || p.Status == PaymentRules.Pending))
                    .SumAsync(p => (decimal?)p.Amount) ?? 0m;
                var remaining = order.TotalAmount - committed;

                if (request.Amount > remaining)
                    throw new InvalidOperationException(PaymentRules.ExceedsBalanceMessage(Math.Max(remaining, 0m)));

                var now = DateTime.UtcNow;
                var userId = _currentTenant.OrganizationUserId;

                var payment = new Payment
                {
                    OrderId = orderId,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    Status = isCash ? PaymentRules.Completed : PaymentRules.Pending,
                    TransactionReference = reference,
                    PaidAtUtc = isCash ? now : null,
                    CreatedAtUtc = now,
                    CreatedByOrganizationUserId = userId,
                    ConfirmedByOrganizationUserId = isCash ? userId : null
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await GetByIdAsync(payment.PaymentId))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<PaymentResponse?> GetByIdAsync(int paymentId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.PaymentId == paymentId)
                .Select(ToResponse)
                .FirstOrDefaultAsync();
        }


        public async Task<List<PaymentResponse>> GetAllByOrderAsync(int orderId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.OrderId == orderId)
                .Select(ToResponse)
                .ToListAsync();
        }

        public async Task<PaymentResponse?> UpdateStatusAsync(int paymentId, UpdatePaymentStatusRequest request)
        {
            if (!PaymentRules.AllowedStatuses.Contains(request.Status))
                throw new InvalidOperationException($"'{request.Status}' is not a valid payment status.");

            var orderId = await _context.Payments
                .Where(p => p.PaymentId == paymentId)
                .Select(p => (int?)p.OrderId)
                .FirstOrDefaultAsync();
            if (orderId is null)
                return null;

            await using var transaction = await _context.BeginTransactionAsync();
            try
            {
                // Same order lock as CreateAsync and the order completion check: a refund and an order completion
                // on the same order cannot interleave.
                await LockOrderAsync(orderId.Value);

                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
                if (payment is null)
                    return null;

                if (!PaymentRules.CanMove(payment.Status, request.Status))
                    throw new InvalidOperationException(PaymentRules.TransitionMessage(payment.Status, request.Status));

                payment.Status = request.Status;

                if (request.Status == PaymentRules.Completed)
                {
                    payment.PaidAtUtc ??= DateTime.UtcNow;
                    payment.ConfirmedByOrganizationUserId = _currentTenant.OrganizationUserId;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetByIdAsync(paymentId);
        }

        private async Task<Order?> LockOrderAsync(int orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order is null)
                return null;

            order.UpdatedAtUtc = DateTime.UtcNow; // an UPDATE holds the row lock until the surrounding transaction ends
            await _context.SaveChangesAsync();
            return order;
        }

        private static readonly System.Linq.Expressions.Expression<Func<Payment, PaymentResponse>> ToResponse = p => new PaymentResponse
        {
            PaymentId = p.PaymentId,
            OrderId = p.OrderId,
            OrderNumber = p.Order.OrderNumber,
            CustomerName = p.Order.Customer == null ? null
                : p.Order.Customer.LastName == null ? p.Order.Customer.FirstName
                : p.Order.Customer.FirstName + " " + p.Order.Customer.LastName,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod,
            Status = p.Status,
            TransactionReference = p.TransactionReference,
            PaidAtUtc = p.PaidAtUtc,
            CreatedAtUtc = p.CreatedAtUtc,
            CreatedByOrganizationUserId = p.CreatedByOrganizationUserId,
            ConfirmedByOrganizationUserId = p.ConfirmedByOrganizationUserId
        };
    }
}
