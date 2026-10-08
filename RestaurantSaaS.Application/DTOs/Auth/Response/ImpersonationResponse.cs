using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.Auth.Response
{
    public class ImpersonationResponse
    {
        public string Token { get; set; } = null!;
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; } = null!;
    }
}
