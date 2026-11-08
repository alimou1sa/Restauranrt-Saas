using RestaurantSaaS.Domain.Common;

namespace RestaurantSaaS.Tests.TestSupport;

/// <summary>
/// A very small <see cref="ICurrentTenant"/> for tests.
/// <see cref="RestaurantSaaS.Infrastructure.Data.AppDbContext"/> uses it to decide which rows a query may see:
/// when <see cref="OrganizationId"/> is null, every tenant table returns zero rows.
/// </summary>
public sealed class FakeCurrentTenant : ICurrentTenant
{
    public int? OrganizationId { get; init; }

    public int? OrganizationUserId { get; init; }

    public int? BranchId { get; init; }
}
