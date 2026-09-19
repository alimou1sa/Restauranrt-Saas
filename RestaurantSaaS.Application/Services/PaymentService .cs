using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Payments;
    using global::RestaurantSaaS.Application.DTOs.Payments.PaymentsRequest;
    using global::RestaurantSaaS.Application.InterfacesService;
    using global::RestaurantSaaS.Infrastructure;
    using Microsoft.EntityFrameworkCore;
namespace RestaurantSaaS.Application.Services
{

    public class PaymentService : IPaymentService
    {
        private static readonly string[] AllowedMethods = { "Cash", "Card", "Online", "Other" };
        private static readonly string[] AllowedStatuses = { "Pending", "Completed", "Failed", "Refunded" };

        private readonly IAppDbContext _context;

        public PaymentService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentResponse> CreateAsync(int orderId, CreatePaymentRequest request)
        {
            var orderExists = await _context.Orders
                .AnyAsync(o => o.OrderId == orderId);

            if (!orderExists)
                throw new KeyNotFoundException($"Order {orderId} was not found.");

            if (!AllowedMethods.Contains(request.PaymentMethod))
                throw new InvalidOperationException($"'{request.PaymentMethod}' is not a valid payment method.");

            var isCash = request.PaymentMethod == "Cash";

            var payment = new Payment
            {
                OrderId = orderId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                Status = isCash ? "Completed" : "Pending",
                TransactionReference = request.TransactionReference,
                PaidAtUtc = isCash ? DateTime.UtcNow : null,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(payment.PaymentId))!;
        }

        public async Task<PaymentResponse?> GetByIdAsync(int paymentId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.PaymentId == paymentId)
                .Select(p => new PaymentResponse
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
                    CreatedAtUtc = p.CreatedAtUtc
                })
                .FirstOrDefaultAsync();
        }


        public async Task<List<PaymentResponse>> GetAllByOrderAsync(int orderId)
        {
            return await _context.Payments
                .AsNoTracking()
                .Where(p => p.OrderId == orderId)
                .Select(p => new PaymentResponse
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
                    CreatedAtUtc = p.CreatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<PaymentResponse?> UpdateStatusAsync(int paymentId, UpdatePaymentStatusRequest request)
        {
            if (!AllowedStatuses.Contains(request.Status))
                throw new InvalidOperationException($"'{request.Status}' is not a valid payment status.");

            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment is null)
                return null;

            payment.Status = request.Status;

            if (request.Status == "Completed" && payment.PaidAtUtc is null)
                payment.PaidAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(paymentId);
        }
    }
}
