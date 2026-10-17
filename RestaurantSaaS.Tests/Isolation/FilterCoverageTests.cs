using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Domain.Common;
using RestaurantSaaS.Infrastructure.Data;
using RestaurantSaaS.Tests.TestSupport;
using Xunit;

namespace RestaurantSaaS.Tests.Isolation;

/// <summary>
/// A static guard rail over the EF model. It fails the build when a tenant-scoped entity is added
/// without a global query filter, or when a new global (unfiltered) entity appears without being
/// consciously added to the documented allow-list. No database is required.
/// </summary>
public sealed class FilterCoverageTests
{
    private static AppDbContext CreateModelOnlyContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=model-only;Trusted_Connection=True;TrustServerCertificate=True")
                .Options,
            new FakeCurrentTenant());

    [Fact]
    public void Every_tenant_entity_has_a_global_query_filter()
    {
        using var ctx = CreateModelOnlyContext();

        var missing = ctx.Model.GetEntityTypes()
            .Where(t => typeof(ITenantEntity).IsAssignableFrom(t.ClrType))
            .Where(t => t.GetQueryFilter() is null)
            .Select(t => t.ClrType.Name)
            .OrderBy(n => n)
            .ToArray();

        Assert.Empty(missing);
    }

    [Fact]
    public void Only_documented_global_entities_are_unfiltered()
    {
        using var ctx = CreateModelOnlyContext();

        var unfiltered = ctx.Model.GetEntityTypes()
            .Where(t => t.GetQueryFilter() is null)
            .Select(t => t.ClrType.Name)
            .OrderBy(n => n)
            .ToArray();

        // Global by design: Organizations/Users/Permissions/Plans/SystemRoles support sign-in and the
        // platform area; RolePermission/SystemRolePermission carry no tenant column and are always
        // reached through a filtered parent; RefreshTokens are read with IgnoreQueryFilters in auth.
        var documentedGlobals = new[]
        {
            "Organization", "Permission", "Plan", "RefreshToken",
            "RolePermission", "SystemRole", "SystemRolePermission", "User"
        };

        Assert.Equal(documentedGlobals.OrderBy(n => n).ToArray(), unfiltered);
    }

    [Theory]
    // Branch-owned rows are reached through a navigation, so their filter must also pin the
    // organization (via Branch.OrganizationId) - otherwise a wrong branch id could leak data.
    [InlineData("Menu")]
    [InlineData("Category")]
    [InlineData("Product")]
    [InlineData("RestaurantTable")]
    [InlineData("Order")]
    [InlineData("OrderItem")]
    [InlineData("Payment")]
    [InlineData("Inventory")]
    [InlineData("InventoryTransaction")]
    public void Branch_owned_entities_filter_on_both_organization_and_branch(string entityName)
    {
        using var ctx = CreateModelOnlyContext();

        var entityType = ctx.Model.GetEntityTypes().Single(t => t.ClrType.Name == entityName);
        var filter = entityType.GetQueryFilter();

        Assert.NotNull(filter);
        Assert.Contains("OrganizationId", filter!.ToString());
        Assert.Contains("Branch", filter.ToString());
    }
}
