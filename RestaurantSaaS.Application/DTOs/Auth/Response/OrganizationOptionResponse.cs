using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.Auth.Response
{
   
        public class OrganizationOptionResponse
        {
            public int OrganizationId { get; set; }
            public string OrganizationName { get; set; } = null!;
            public int? BranchId { get; set; }
            public string? BranchName { get; set; }
        
    }
}
