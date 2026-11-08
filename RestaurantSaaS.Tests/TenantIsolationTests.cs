using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Tests.TestSupport;
using Xunit;

namespace RestaurantSaaS.Tests;

/// <summary>
/// Proves that an organization can only see its own data.
/// Two organizations are created; each one must see its own order and nothing else.
/// </summary>
public sealed class TenantIsolationTests : IAsyncLifetime
{
    private readonly TestDatabase _db = new();
    private TestIds _ids = null!;
    private int _ownOrderId;
    private int _otherOrderId;

    public async Task InitializeAsync()
    {
        await _db.CreateAsync();
        _ids = await _db.SeedAsync();

        // An order for the seeded organization.
        _ownOrderId = await _db.SeedOrderAsync(_ids);

        // A second organization with its own branch and order.
        await using var context = _db.CreateContext();

        var otherOrganization = new Organization
        {
            Name = "Other Organization",
            Slug = "other-organization",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        context.Organizations.Add(otherOrganization);
        await context.SaveChangesAsync();

        var otherBranch = new Branch
        {
            OrganizationId = otherOrganization.OrganizationId,
            Name = "Other Branch",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        context.Branches.Add(otherBranch);
        await context.SaveChangesAsync();

        var otherOrder = new Order
        {
            OrganizationId = otherOrganization.OrganizationId,
            BranchId = otherBranch.BranchId,
            OrderNumber = "ORD-OTHER-1",
            Status = "Pending",
            OrderType = "Takeaway",
            CreatedAtUtc = DateTime.UtcNow
        };
        context.Orders.Add(otherOrder);
        await context.SaveChangesAsync();

        _otherOrderId = otherOrder.OrderId;
    }

    public async Task DisposeAsync() => await _db.DisposeAsync();

    [Fact]
    public async Task An_organization_sees_only_its_own_orders()
    {
        await using var context = _db.CreateContext(_ids.OrganizationId);

        var visibleOrderIds = await context.Orders.Select(o => o.OrderId).ToListAsync();

        Assert.Contains(_ownOrderId, visibleOrderIds);
        Assert.DoesNotContain(_otherOrderId, visibleOrderIds);
        Assert.Single(visibleOrderIds);
    }

    [Fact]
    public async Task Without_an_organization_nothing_is_visible()
    {
        await using var context = _db.CreateContext();

        Assert.Empty(await context.Orders.ToListAsync());
    }
}
