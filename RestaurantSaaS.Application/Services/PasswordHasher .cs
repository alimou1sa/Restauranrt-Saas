using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 using RestaurantSaaS.Application.InterfacesService;
 using System.Security.Cryptography;

namespace RestaurantSaaS.Application.Services
{

    public class PasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool Verify(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
    }
}
