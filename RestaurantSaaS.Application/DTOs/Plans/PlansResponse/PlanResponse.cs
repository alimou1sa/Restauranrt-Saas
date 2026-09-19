using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.Plans.PlansResponse
{

    public class PlanResponse
    {
        public int PlanId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public decimal MonthlyPrice { get; set; }

        public decimal YearlyPrice { get; set; }

        public int? MaxBranches { get; set; }

        public int? MaxUsers { get; set; }

        public int? MaxProducts { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
