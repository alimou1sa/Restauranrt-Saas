using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.Orders.OrdersRequest
{


    public class UpdateOrderStatusRequest
    {
        [Required, MaxLength(30)]
        public string Status { get; set; } = null!;
    }
}
