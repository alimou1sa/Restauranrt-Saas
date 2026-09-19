using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.Orders.OrdersRequest
{


    // BranchId comes from the route/tenant context. OrderNumber is generated
    // server-side (a per-branch sequence/business rule), never supplied by the
    // client. Status defaults server-side to the initial open state. All
    // monetary fields (SubTotal/DiscountAmount/TaxAmount/TotalAmount) are
    // excluded - they are calculated from OrderItems, not sent at creation.
    public class CreateOrderRequest
    {
        [Required, MaxLength(20)]
        public string OrderType { get; set; } = null!;

        public int? TableId { get; set; }

        public int? CustomerId { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
