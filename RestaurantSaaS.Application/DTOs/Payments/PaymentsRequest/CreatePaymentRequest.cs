using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.Payments.PaymentsRequest
{

    // OrderId comes from the route (e.g. POST /orders/{orderId}/payments).
    // Status is not client-supplied - the server determines it based on the
    // payment method / gateway result (e.g. cash is immediately "Completed",
    // a card payment may start "Pending" until gateway confirmation).
    // PaidAtUtc is stamped server-side once the payment is confirmed.
    public class CreatePaymentRequest
    {
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required, MaxLength(30)]
        public string PaymentMethod { get; set; } = null!;

        [MaxLength(200)]
        public string? TransactionReference { get; set; }
    }
}
