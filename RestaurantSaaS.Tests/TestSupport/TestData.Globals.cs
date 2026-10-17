using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Infrastructure.Data;

namespace RestaurantSaaS.Tests.TestSupport;

public static partial class TestData
{
    private static async Task<int> SeedGlobalsAsync(AppDbContext ctx)
    {
        var plan = new Plan
        {
            Name = "Basic",
            Description = "Basic plan",
            MonthlyPrice = 10m,
            YearlyPrice = 100m,
            IsActive = true,
            CreatedAtUtc = Now
        };
        var permission = new Permission { Code = "order.read", Name = "Read orders", Description = "Can read orders" };
        var systemRole = new SystemRole { Name = "Administrator", Description = "Built-in administrator role" };

        ctx.AddRange(plan, permission, systemRole);
        await ctx.SaveChangesAsync();

        return plan.PlanId;
    }

    private static async Task<(Organization A, Organization B)> SeedOrganizationsAsync(AppDbContext ctx)
    {
        var orgA = new Organization { Name = "Org A", Slug = "org-a", IsActive = true, CreatedAtUtc = Now };
        var orgB = new Organization { Name = "Org B", Slug = "org-b", IsActive = true, CreatedAtUtc = Now };

        ctx.AddRange(orgA, orgB);
        await ctx.SaveChangesAsync();

        return (orgA, orgB);
    }
}
