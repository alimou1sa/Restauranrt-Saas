using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.RestaurantTables.RestaurantTablesRequest
{


    // BranchId comes from the route/tenant context. QRCodeToken is never
    // client-supplied - it is DB-generated (DEFAULT newid()) to guarantee
    // uniqueness and unpredictability.
    public class CreateRestaurantTableRequest
    {
        [Required, MaxLength(30)]
        public string TableNumber { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }
    }
}
