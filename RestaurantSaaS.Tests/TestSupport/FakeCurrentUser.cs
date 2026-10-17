using RestaurantSaaS.Domain.Common;

namespace RestaurantSaaS.Tests.TestSupport;

/// <summary>
/// A settable <see cref="ICurrentUser"/> used to simulate the authenticated principal that
/// <c>CurrentUser</c> would normally read from the JWT claims of the current request.
/// </summary>
public sealed class FakeCurrentUser : ICurrentUser
{
    public int UserId { get; init; }

    public string? Jti { get; init; }

    public bool IsAuthenticated { get; init; } = true;

    public DateTime? ExpiresAtUtc { get; init; }

    public bool IsPlatformAdmin { get; init; }
}
