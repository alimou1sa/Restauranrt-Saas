using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
   using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsRequest
{

    // OrderId comes from the route (e.g. POST /orders/{orderId}/items).
    // ProductName and UnitPrice are deliberately excluded: the server must
    // snapshot them from the actual Product at add-time, never trust them
    // from the client (prevents price tampering). LineTotal is DB-computed
    // and never part of a request.
    public class CreateOrderItemRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Range(0.001, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; }
    }
}
