using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
  using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.Payments.PaymentsRequest
{
  



    // A dedicated DTO for status transitions only (e.g. Pending -> Completed /
    // Failed / Refunded), typically driven by a payment gateway callback.
    // No general UpdatePaymentRequest exists: a payment is a financial record
    // and correcting one should be a reversal/refund transaction, not a
    // direct edit of Amount or PaymentMethod.
    public class UpdatePaymentStatusRequest
    {
        [Required, MaxLength(30)]
        public string Status { get; set; } = null!;
    }
}
