using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
   using Microsoft.AspNetCore.Http;
    using RestaurantSaaS.Application.Common;
    using RestaurantSaaS.Application.InterfacesService;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
namespace RestaurantSaaS.Application.Services
{
 
        public class CurrentUser : ICurrentUser
        {
            private readonly ClaimsPrincipal? _user;

            public CurrentUser(IHttpContextAccessor accessor)
            {
                _user = accessor.HttpContext?.User;
            }

            public bool IsAuthenticated => _user?.Identity?.IsAuthenticated ?? false;

            public int UserId => ParseIntClaim(JwtRegisteredClaimNames.Sub)
                ?? throw new InvalidOperationException("No authenticated user in context.");

            public int? OrganizationId => ParseIntClaim(AppClaimTypes.OrganizationId);

            public int? OrganizationUserId => ParseIntClaim(AppClaimTypes.OrganizationUserId);

            public int? BranchId => ParseIntClaim(AppClaimTypes.BranchId);

            private int? ParseIntClaim(string claimType)
            {
                var value = _user?.FindFirstValue(claimType);
                return int.TryParse(value, out var result) ? result : null;
            }
        }
    
}
