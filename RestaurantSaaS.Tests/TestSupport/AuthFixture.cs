namespace RestaurantSaaS.Tests.TestSupport;

/// <summary>
/// A fresh database + seed + auth harness. A new instance is created per test (via
/// <see cref="CreateAsync"/>) so tests that deactivate an organization or a user can never
/// affect a sibling test.
/// </summary>
public sealed class AuthFixture : IAsyncDisposable
{
    public const string Password = "Passw0rd!123";

    private AuthFixture(IsolationDatabase db, SeededWorld world, AuthHarness harness)
    {
        Db = db;
        World = world;
        Harness = harness;
    }

    public IsolationDatabase Db { get; }

    public SeededWorld World { get; }

    public AuthHarness Harness { get; }

    public static async Task<AuthFixture> CreateAsync()
    {
        var db = new IsolationDatabase();
        await db.InitializeAsync();
        var world = await TestData.SeedAsync(db);
        var harness = new AuthHarness(db);
        await harness.SetPasswordAsync(world.OrgA.UserId, Password);
        await harness.SetPasswordAsync(world.OrgB.UserId, Password);
        return new AuthFixture(db, world, harness);
    }

    public string EmailOfA => "user-a@example.com";

    public string EmailOfB => "user-b@example.com";

    public async ValueTask DisposeAsync() => await Db.DisposeAsync();
}
