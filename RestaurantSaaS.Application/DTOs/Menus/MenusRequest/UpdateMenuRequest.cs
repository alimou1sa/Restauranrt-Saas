using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
   using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.Menus.MenusRequest
{
 


    public class UpdateMenuRequest
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsPublished { get; set; }

        public bool IsActive { get; set; }
    }
}
