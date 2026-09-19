using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.Products.ProductsRequest
{
  

    // A lightweight, dedicated DTO for the "sold out" / "back in stock" toggle -
    // this happens far more frequently than a full product edit and deserves
    // its own small, fast endpoint (mirrors UpdateOrderStatusRequest).
    public class UpdateProductAvailabilityRequest
    {
        public bool IsAvailable { get; set; }
    }
}
