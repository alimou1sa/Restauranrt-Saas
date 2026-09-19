using System;
using System.Collections.Generic;

namespace RestaurantSaaS.Infrastructure;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? TransactionReference { get; set; }

    public DateTime? PaidAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual Order Order { get; set; } = null!;
}
