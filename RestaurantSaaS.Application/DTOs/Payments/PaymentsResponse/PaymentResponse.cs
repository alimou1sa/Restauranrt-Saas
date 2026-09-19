namespace RestaurantSaaS.Application.DTOs.Payments;

public class PaymentResponse
{
    public long PaymentId { get; set; }

    public long OrderId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public string? CustomerName { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? TransactionReference { get; set; }

    public DateTime? PaidAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
