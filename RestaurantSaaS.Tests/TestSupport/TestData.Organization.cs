using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Infrastructure.Data;

namespace RestaurantSaaS.Tests.TestSupport;

public static partial class TestData
{
    private static async Task<OrgSeed> SeedOrganizationAsync(
        AppDbContext ctx, int planId, string label, Organization org, string[] branchKeys)
    {
        var seed = new OrgSeed { OrganizationId = org.OrganizationId };

        // Branches (tenant owned by organization).
        var branches = new Dictionary<string, Branch>();
        foreach (var key in branchKeys)
        {
            var branch = new Branch
            {
                OrganizationId = org.OrganizationId,
                Name = key,
                IsActive = true,
                CreatedAtUtc = Now
            };
            branches[key] = branch;
            ctx.Add(branch);
        }
        await ctx.SaveChangesAsync();

        // Organization owned rows: customer, custom role, user, membership, subscription, user-role.
        var customer = new Customer { OrganizationId = org.OrganizationId, FirstName = $"Customer {label}", CreatedAtUtc = Now };
        var role = new Role { OrganizationId = org.OrganizationId, Name = "Manager", IsActive = true, CreatedAtUtc = Now };
        var user = new User
        {
            FirstName = "User",
            LastName = label,
            Email = $"user-{label.ToLowerInvariant()}@example.com",
            PasswordHash = "not-a-real-hash",
            IsActive = true,
            CreatedAtUtc = Now
        };
        ctx.AddRange(customer, role, user);
        await ctx.SaveChangesAsync();

        var firstBranch = branches[branchKeys[0]];
        var membership = new OrganizationUser
        {
            OrganizationId = org.OrganizationId,
            UserId = user.UserId,
            BranchId = firstBranch.BranchId,
            IsActive = true,
            JoinedAtUtc = Now
        };
        var subscription = new Subscription
        {
            OrganizationId = org.OrganizationId,
            PlanId = planId,
            Status = "Active",
            BillingCycle = "Monthly",
            StartDateUtc = Now,
            CurrentPeriodStartUtc = Now,
            CurrentPeriodEndUtc = Now.AddMonths(1),
            CreatedAtUtc = Now
        };
        ctx.AddRange(membership, subscription);
        await ctx.SaveChangesAsync();

        var userRole = new UserRole
        {
            OrganizationId = org.OrganizationId,
            OrganizationUserId = membership.OrganizationUserId,
            RoleId = role.RoleId,
            AssignedAtUtc = Now
        };
        ctx.Add(userRole);
        await ctx.SaveChangesAsync();

        seed.UserId = user.UserId;
        seed.RoleId = role.RoleId;
        seed.CustomerId = customer.CustomerId;
        seed.OrganizationUserId = membership.OrganizationUserId;
        seed.SubscriptionId = subscription.SubscriptionId;
        seed.UserRoleId = userRole.UserRoleId;

        foreach (var key in branchKeys)
        {
            seed.Branches[key] = await SeedBranchGraphAsync(ctx, org, branches[key], key);
        }

        return seed;
    }
}
