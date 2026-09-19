using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.Orders.OrdersRequest
{

    // Covers editable order details only. Status transitions go through
    // UpdateOrderStatusRequest, and monetary totals are always server-computed.
    public class UpdateOrderRequest
    {
        public int? TableId { get; set; }

        public int? CustomerId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // The one monetary field a staff member may set manually (a discount
        // applied to the order); SubTotal/TaxAmount/TotalAmount stay server-computed.
        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; }
    }
}
