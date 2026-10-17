namespace RestaurantSaaS.Tests.TestSupport;

/// <summary>A fresh RBAC database per test (created via <see cref="CreateAsync"/>).</summary>
public sealed class PermissionFixture : IAsyncDisposable
{
    private PermissionFixture(IsolationDatabase db, PermissionWorld world)
    {
        Db = db;
        World = world;
    }

    public IsolationDatabase Db { get; }

    public PermissionWorld World { get; }

    public static async Task<PermissionFixture> CreateAsync()
    {
        var db = new IsolationDatabase();
        await db.InitializeAsync();
        var world = await PermissionData.SeedAsync(db);
        return new PermissionFixture(db, world);
    }

    public async ValueTask DisposeAsync() => await Db.DisposeAsync();
}
