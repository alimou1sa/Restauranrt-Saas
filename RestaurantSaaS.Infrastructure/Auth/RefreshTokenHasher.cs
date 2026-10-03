using RestaurantSaaS.Application.InterfacesService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
namespace RestaurantSaaS.Infrastructure.Auth
{


    public class RefreshTokenHasher : IRefreshTokenHasher   
    {
        public string GenerateRawToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        public string Hash(string rawToken)
        {
            var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToBase64String(bytes);
        }
    }

}
