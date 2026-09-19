using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.Plans.PlansRequest
{
   


    public class UpdatePlanRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MonthlyPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal YearlyPrice { get; set; }

        [Range(1, int.MaxValue)]
        public int? MaxBranches { get; set; }

        [Range(1, int.MaxValue)]
        public int? MaxUsers { get; set; }

        [Range(1, int.MaxValue)]
        public int? MaxProducts { get; set; }

        public bool IsActive { get; set; }
    }
}
