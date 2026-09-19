using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
   using System.ComponentModel.DataAnnotations;
namespace RestaurantSaaS.Application.DTOs.Auth.Request
{
 

        public class SelectOrganizationRequest
        {
            [Required]
            public int OrganizationId { get; set; }
        }
    
}
