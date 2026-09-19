using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.Auth.Response
{

        public class LoginResponse
        {
            public string Token { get; set; } = null!;
            public string TokenType { get; set; } = null!;
            public List<OrganizationOptionResponse> Organizations { get; set; } = new();
        }
    
}
