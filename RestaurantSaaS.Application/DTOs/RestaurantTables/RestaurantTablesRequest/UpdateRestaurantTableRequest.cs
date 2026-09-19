using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.RestaurantTables.RestaurantTablesRequest
{

    public class UpdateRestaurantTableRequest
    {
        [Required, MaxLength(30)]
        public string TableNumber { get; set; } = null!;

        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }

        public bool IsActive { get; set; }
    }
}
