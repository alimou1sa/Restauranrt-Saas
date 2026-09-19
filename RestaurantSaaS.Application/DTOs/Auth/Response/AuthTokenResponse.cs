using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.Auth.Response
{
   
        public class AuthTokenResponse
        {
            public string Token { get; set; } = null!;
            public int OrganizationId { get; set; }
            public int OrganizationUserId { get; set; }
            public int? BranchId { get; set; }
        }
    
}
