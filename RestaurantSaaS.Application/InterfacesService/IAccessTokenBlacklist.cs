using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.InterfacesService
{
    public interface IAccessTokenBlacklist
    {
        void Revoke(string jti, TimeSpan remainingLifetime);
        bool IsRevoked(string jti);
    }
}
