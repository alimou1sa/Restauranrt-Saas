using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using System.ComponentModel.DataAnnotations;

namespace RestaurantSaaS.Application.DTOs.Customers.CustomersRequest
{

    public class UpdateCustomerRequest
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = null!;

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }

        [MaxLength(320), EmailAddress]
        public string? Email { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
