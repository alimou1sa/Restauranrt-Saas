using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;

namespace RestaurantSaaS.Application.DTOs.Categories.CategoriesRequest
{

    public class UpdateCategoryRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
