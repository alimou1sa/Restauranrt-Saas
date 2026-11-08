using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Infrastructure.Data;

namespace RestaurantSaaS.Tests.TestSupport;

/// <summary>
/// A small, throwaway SQL Server database (LocalDB) used by the tests.
///
/// Why a real database and not an in-memory list: <see cref="AppDbContext"/> hides rows that belong to
/// another organization through query filters. Only a real database proves that behaviour.
///
/// Every test class creates its own database (with a unique name) and deletes it when it finishes,
/// so test classes can run in parallel and never see each other's data.
/// </summary>
public sealed class TestDatabase : IAsyncDisposable
{
    private const string Server = @"(localdb)\MSSQLLocalDB";

    public TestDatabase()
    {
        var databaseName = "RestaurantSaaS_Tests_" + Guid.NewGuid().ToString("N");
        ConnectionString =
            $"Server={Server};Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
    }

    public string ConnectionString { get; }

    /// <summary>EF Core options that point at this throwaway database.</summary>
    public DbContextOptions<AppDbContext> Options =>
        new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(ConnectionString).Options;

    /// <summary>
    /// Creates a context. Pass an organization id to act like a signed-in user of that organization:
    /// the context then only returns that organization's data.
    /// With no organization id, every table returns zero rows (default deny).
    /// </summary>
    public AppDbContext CreateContext(int? organizationId = null) =>
        new(Options, new FakeCurrentTenant { OrganizationId = organizationId });

    /// <summary>Creates the tables. Call this once at the start of a test class.</summary>
    public async Task CreateAsync()
    {
        await using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Inserts the data every test can start from: one organization, one branch,
    /// one menu, one category, one product, one table and one customer.
    /// </summary>
    public async Task<TestIds> SeedAsync()
    {
        await using var context = CreateContext();
        var now = DateTime.UtcNow;

        var organization = new Organization
        {
            Name = "Test Organization",
            Slug = "test-organization",
            IsActive = true,
            CreatedAtUtc = now
        };
        context.Organizations.Add(organization);
        await context.SaveChangesAsync();

        var branch = new Branch
        {
            OrganizationId = organization.OrganizationId,
            Name = "Main Branch",
            IsActive = true,
            CreatedAtUtc = now
        };
        context.Branches.Add(branch);
        await context.SaveChangesAsync();

        var menu = new Menu
        {
            BranchId = branch.BranchId,
            Name = "Main Menu",
            IsActive = true,
            IsPublished = true,
            CreatedAtUtc = now
        };
        context.Menus.Add(menu);
        await context.SaveChangesAsync();

        var category = new Category
        {
            MenuId = menu.MenuId,
            Name = "Food",
            DisplayOrder = 0,
            IsActive = true
        };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var product = new Product
        {
            OrganizationId = organization.OrganizationId,
            CategoryId = category.CategoryId,
            Name = "Burger",
            Price = 10m,
            IsActive = true,
            IsAvailable = true,
            CreatedAtUtc = now
        };
        var table = new RestaurantTable
        {
            BranchId = branch.BranchId,
            TableNumber = "T1",
            Capacity = 4,
            QRCodeToken = Guid.NewGuid(),
            IsActive = true,
            CreatedAtUtc = now
        };
        var customer = new Customer
        {
            OrganizationId = organization.OrganizationId,
            FirstName = "Ali",
            CreatedAtUtc = now
        };
        context.Products.Add(product);
        context.RestaurantTables.Add(table);
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        return new TestIds(
            organization.OrganizationId,
            branch.BranchId,
            product.ProductId,
            table.TableId,
            customer.CustomerId);
    }

    /// <summary>Inserts an order directly (without the order service) so a test can start from any total or status.</summary>
    public async Task<int> SeedOrderAsync(TestIds ids, decimal total = 20m, string status = "Pending")
    {
        await using var context = CreateContext();

        var order = new Order
        {
            OrganizationId = ids.OrganizationId,
            BranchId = ids.BranchId,
            OrderNumber = "ORD-" + Guid.NewGuid().ToString("N")[..8],
            Status = status,
            OrderType = "Takeaway",
            SubTotal = total,
            DiscountAmount = 0m,
            TaxAmount = 0m,
            TotalAmount = total,
            CreatedAtUtc = DateTime.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return order.OrderId;
    }

    /// <summary>Deletes the throwaway database. A cleanup failure never fails a test.</summary>
    public async ValueTask DisposeAsync()
    {
        try
        {
            await using var context = CreateContext();
            await context.Database.EnsureDeletedAsync();
        }
        catch
        {
            // Best effort only.
        }
    }
}

/// <summary>The ids created by <see cref="TestDatabase.SeedAsync"/>.</summary>
public sealed record TestIds(
    int OrganizationId,
    int BranchId,
    int ProductId,
    int TableId,
    int CustomerId);
