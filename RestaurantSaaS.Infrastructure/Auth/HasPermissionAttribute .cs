using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
   using Microsoft.AspNetCore.Authorization;
namespace RestaurantSaaS.Infrastructure.Auth
{
 

        public class HasPermissionAttribute : AuthorizeAttribute
        {
            public HasPermissionAttribute(string permissionCode)
            {
                Policy = $"Permission:{permissionCode}";
            }
        }
    
}
