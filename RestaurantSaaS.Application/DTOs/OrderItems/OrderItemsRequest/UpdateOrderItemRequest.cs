using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsRequest
{



    // ProductId is not editable here: to change the product, remove this line
    // and add a new one. Only quantity/discount can be adjusted on an
    // existing line.
    public class UpdateOrderItemRequest
    {
        [Range(0.001, double.MaxValue)]
        public decimal Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DiscountAmount { get; set; }
    }
}
