using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Infrastructure.Auth
{
    using global::RestaurantSaaS.Application.InterfacesService;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;

    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    namespace RestaurantSaaS.Infrastructure.Auth
    {
        public class JwtTokenGenerator : IJwtTokenGenerator
        {
            private readonly JwtSettings _settings;

            public JwtTokenGenerator(IOptions<JwtSettings> options)
            {
                _settings = options.Value;
            }

            public string GenerateToken(List<Claim> claims, TimeSpan? expiration = null)
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var expiresAtUtc = DateTime.UtcNow.Add(
                    expiration ?? TimeSpan.FromMinutes(_settings.ExpirationMinutes));

                var token = new JwtSecurityToken(
                    issuer: _settings.Issuer,
                    audience: _settings.Audience,
                    claims: claims,
                    expires: expiresAtUtc,
                    signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
        }
    }
}
