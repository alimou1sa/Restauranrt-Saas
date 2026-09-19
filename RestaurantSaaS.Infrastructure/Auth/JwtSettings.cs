using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Infrastructure.Auth
{
    public class JwtSettings
    {
        public string Key { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int ExpirationMinutes { get; set; }
    }
}
