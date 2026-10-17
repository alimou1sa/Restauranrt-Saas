using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Infrastructure.Data;

namespace RestaurantSaaS.Tests.TestSupport;

/// <summary>
/// Owns a throwaway SQL Server database (LocalDB) for one test class.
///
/// Why a real database instead of the EF InMemory provider: the isolation rules under test live in
/// <see cref="AppDbContext"/> as global query filters, and the point of these tests is to prove that
/// the filters really restrict what a tenant/branch context can read. A real provider (with the real
/// schema produced by <c>EnsureCreated</c>, including the SQL Server specific defaults, computed
/// columns and filtered indexes) is the only way to be sure we are testing the production behaviour.
///
/// A new, uniquely named database is created per test class, so test classes can run in parallel.
/// </summary>
public sealed class IsolationDatabase : IAsyncDisposable
{
    private const string Server = @"(localdb)\MSSQLLocalDB";

    public IsolationDatabase()
    {
        DatabaseName = "RestaurantSaaS_Isolation_" + Guid.NewGuid().ToString("N");
        ConnectionString =
            $"Server={Server};Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
    }

    public string DatabaseName { get; }

    public string ConnectionString { get; }

    public DbContextOptions<AppDbContext> BuildOptions() =>
        new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(ConnectionString).Options;

    /// <summary>
    /// Creates a context bound to a tenant/branch context. All arguments are optional; with the
    /// defaults every tenant-scoped table returns zero rows (the default-deny behaviour).
    /// </summary>
    public AppDbContext CreateContext(int? organizationId = null, int? organizationUserId = null, int? branchId = null) =>
        CreateContext(new FakeCurrentTenant
        {
            OrganizationId = organizationId,
            OrganizationUserId = organizationUserId,
            BranchId = branchId
        });

    /// <summary>Creates a context bound to an explicit tenant object (so services can share it).</summary>
    public AppDbContext CreateContext(FakeCurrentTenant tenant) => new(BuildOptions(), tenant);

    /// <summary>Creates a tenant object that can be handed to both a context and a service under test.</summary>
    public static FakeCurrentTenant NewTenant(int? organizationId = null, int? organizationUserId = null, int? branchId = null) =>
        new()
        {
            OrganizationId = organizationId,
            OrganizationUserId = organizationUserId,
            BranchId = branchId
        };

    /// <summary>Creates the schema for this test class. Safe to call once per class.</summary>
    public async Task InitializeAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    /// <summary>Drops and recreates the schema (used by tests that mutate the shared seed).</summary>
    public async Task ResetAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await using var context = CreateContext();
            await context.Database.EnsureDeletedAsync();
        }
        catch
        {
            // Best-effort cleanup: a leftover local test database must never fail a test run.
        }
    }
}
