using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.Products.ProductsRequest
{
    


    public class UpdateProductRequest
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [MaxLength(1000)]
        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
