using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 using System.Security.Claims;

namespace RestaurantSaaS.Application.InterfacesService
{
   

        public interface IJwtTokenGenerator
        {
            string GenerateToken(List<Claim> claims, TimeSpan? expiration = null);
        }
    
}
