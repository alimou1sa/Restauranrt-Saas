using RestaurantSaaS.Domain.Common;

namespace RestaurantSaaS.Tests.TestSupport;

/// <summary>
/// A settable <see cref="ICurrentTenant"/> used to simulate the tenant/branch context that
/// <c>CurrentTenant</c> would normally read from the JWT claims of the current request.
/// Each <c>AppDbContext</c> built by <see cref="IsolationDatabase"/> gets its own instance so
/// two contexts in the same test can represent two different tenants at the same time.
/// </summary>
public sealed class FakeCurrentTenant : ICurrentTenant
{
    public int? OrganizationId { get; init; }

    public int? OrganizationUserId { get; init; }

    public int? BranchId { get; init; }
}
