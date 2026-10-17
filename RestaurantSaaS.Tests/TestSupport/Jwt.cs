using System.IdentityModel.Tokens.Jwt;

namespace RestaurantSaaS.Tests.TestSupport;

/// <summary>Reads the raw claims from a token the API issued, for assertions.</summary>
public static class Jwt
{
    public static JwtSecurityToken Read(string token) => new JwtSecurityTokenHandler().ReadJwtToken(token);

    public static string? Claim(string token, string claimType) =>
        Read(token).Claims.FirstOrDefault(c => c.Type == claimType)?.Value;

    public static bool HasClaim(string token, string claimType) => Claim(token, claimType) is not null;
}
