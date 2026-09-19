using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace RestaurantSaaS.Application.DTOs.Organizations.OrganizationsRequest
{


    public class CreateOrganizationRequest
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Slug { get; set; } = null!;

        [MaxLength(320), EmailAddress]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? Phone { get; set; }
    }
}
