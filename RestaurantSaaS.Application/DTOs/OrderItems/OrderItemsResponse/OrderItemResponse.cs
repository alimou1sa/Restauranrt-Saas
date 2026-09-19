namespace RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsResponse;

// OrderId removed: an OrderItem is only ever consumed nested inside an
// OrderResponse.Items list, so the parent order is already known from
// context - repeating its id on every line item is redundant.
// ProductId is kept (a real, independently useful reference, e.g. to link
// back to the product page or re-order it).
public class OrderItemResponse
{
    public int OrderItemId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public decimal Quantity { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal LineTotal { get; set; }
}