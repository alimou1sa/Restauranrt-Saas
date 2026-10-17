using RestaurantSaaS.Domain.Entities;

namespace RestaurantSaaS.Tests.TestSupport;

public static partial class TestData
{
    public static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static async Task<SeededWorld> SeedAsync(IsolationDatabase db)
    {
        await using var ctx = db.CreateContext();
        var world = new SeededWorld();

        world.PlanId = await SeedGlobalsAsync(ctx);
        var (orgA, orgB) = await SeedOrganizationsAsync(ctx);

        world.OrgA = await SeedOrganizationAsync(ctx, world.PlanId, "A", orgA, new[] { "A1", "A2" });
        world.OrgB = await SeedOrganizationAsync(ctx, world.PlanId, "B", orgB, new[] { "B1" });

        return world;
    }
}
