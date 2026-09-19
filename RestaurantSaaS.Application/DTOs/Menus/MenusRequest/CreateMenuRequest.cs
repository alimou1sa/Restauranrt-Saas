using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;


namespace RestaurantSaaS.Application.DTOs.Menus.MenusRequest

{


    // BranchId comes from the route/tenant context (e.g. POST /branches/{branchId}/menus).
    public class CreateMenuRequest
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}