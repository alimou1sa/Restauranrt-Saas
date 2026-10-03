using Microsoft.AspNetCore.Http;
using RestaurantSaaS.Application.Common;
using RestaurantSaaS.Domain.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RestaurantSaaS.Infrastructure.Auth;

public class CurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal? _user;

    public CurrentUser(IHttpContextAccessor accessor)
    {
        _user = accessor.HttpContext?.User;
    }

    public bool IsAuthenticated =>
        _user?.Identity?.IsAuthenticated ?? false;
    /*
    public int UserId =>ParseIntClaim(JwtRegisteredClaimNames.Sub)?? throw
        new InvalidOperationException("No authenticated user in context.");

    private int? ParseIntClaim(string claimType)
    {
        var value = _user?.FindFirstValue(claimType);

        return int.TryParse(value, out var result)? result: null;
    }
    */


    public int UserId => int.TryParse(_user?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id)
        ? id : throw new InvalidOperationException("No authenticated user in context.");

    public string? Jti => _user?.FindFirstValue(JwtRegisteredClaimNames.Jti);

    public DateTime? ExpiresAtUtc
    {
        get
        {
            var exp = _user?.FindFirstValue(JwtRegisteredClaimNames.Exp);
            return long.TryParse(exp, out var unixSeconds)
                ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime
                : null;
        }
    }


}
