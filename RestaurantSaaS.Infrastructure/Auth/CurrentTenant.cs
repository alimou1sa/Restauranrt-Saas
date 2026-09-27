using Microsoft.AspNetCore.Http;
using RestaurantSaaS.Application.Common;
using RestaurantSaaS.Domain.Common;
using System.Security.Claims;

namespace RestaurantSaaS.Infrastructure.Auth;

public class CurrentTenant : ICurrentTenant
{
    private readonly ClaimsPrincipal? _user;

    public CurrentTenant(IHttpContextAccessor accessor)
    {
        _user = accessor.HttpContext?.User;
    }

    public int? OrganizationId =>ParseIntClaim(AppClaimTypes.OrganizationId);

    public int? OrganizationUserId =>ParseIntClaim(AppClaimTypes.OrganizationUserId);

    public int? BranchId =>ParseIntClaim(AppClaimTypes.BranchId);

    private int? ParseIntClaim(string claimType)
    {
        var value = _user?.FindFirstValue(claimType);

        return int.TryParse(value, out var result)? result: null;
    }
}
