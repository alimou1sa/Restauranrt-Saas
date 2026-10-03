using Microsoft.Extensions.Caching.Memory;
using RestaurantSaaS.Application.InterfacesService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.Services
{
    public class AccessTokenBlacklist : IAccessTokenBlacklist
    {
        private const string Prefix = "revoked-jti:";
        private readonly IMemoryCache _cache;

        public AccessTokenBlacklist(IMemoryCache cache) => _cache = cache;

        public void Revoke(string jti, TimeSpan remainingLifetime)
        {
            if (remainingLifetime > TimeSpan.Zero)
                _cache.Set(Prefix + jti, true, remainingLifetime);   
        }

        public bool IsRevoked(string jti) => _cache.TryGetValue(Prefix + jti, out _);
    }
}
