using RestaurantSaaS.Domain.Entities;
using RestaurantSaaS.Infrastructure.Data;

namespace RestaurantSaaS.Tests.TestSupport;

/// <summary>Ids created by <see cref="PermissionData"/> for the RBAC tests.</summary>
public sealed class PermissionWorld
{
    public int OrderReadId { get; set; }
    public int OrderManageId { get; set; }
    public int SystemRoleId { get; set; }

    public int OrgAId { get; set; }
    public int OrgBId { get; set; }

    public int SystemUserMembershipId { get; set; }      // has a system role -> order.read
    public int CustomUserMembershipId { get; set; }      // has a custom role -> order.manage
    public int NoRoleMembershipId { get; set; }          // no roles -> nothing
    public int InactiveMembershipId { get; set; }        // role granted, but the membership is inactive
    public int RemovedMembershipId { get; set; }         // role granted, but the membership was removed
    public int InactiveRoleMembershipId { get; set; }    // role granted, but the role is inactive
    public int OrgBMembershipId { get; set; }            // a different organization, no roles
}

/// <summary>
/// Builds a small RBAC world: a permission catalog, a system role with permissions, a custom role
/// with permissions, and memberships that exercise every "should be denied" path.
/// </summary>
public static class PermissionData
{
    private const string SystemRoleName = "Owner";

    public static async Task<PermissionWorld> SeedAsync(IsolationDatabase db)
    {
        await using var ctx = db.CreateContext();
        var world = new PermissionWorld();

        var orderRead = new Permission { Code = "order.read", Name = "Read orders" };
        var orderManage = new Permission { Code = "order.manage", Name = "Manage orders" };
        var owner = new SystemRole { Name = SystemRoleName };
        ctx.AddRange(orderRead, orderManage, owner);
        await ctx.SaveChangesAsync();

        world.OrderReadId = orderRead.PermissionId;
        world.OrderManageId = orderManage.PermissionId;
        world.SystemRoleId = owner.SystemRoleId;

        ctx.Add(new SystemRolePermission { SystemRoleId = owner.SystemRoleId, PermissionId = orderRead.PermissionId });
        await ctx.SaveChangesAsync();

        var orgA = new Organization { Name = "Perm A", Slug = "perm-a", IsActive = true, CreatedAtUtc = TestData.Now };
        var orgB = new Organization { Name = "Perm B", Slug = "perm-b", IsActive = true, CreatedAtUtc = TestData.Now };
        ctx.AddRange(orgA, orgB);
        await ctx.SaveChangesAsync();
        world.OrgAId = orgA.OrganizationId;
        world.OrgBId = orgB.OrganizationId;

        var systemRoleInstance = new Role { OrganizationId = orgA.OrganizationId, SystemRoleId = owner.SystemRoleId, IsActive = true, CreatedAtUtc = TestData.Now };
        var customManager = new Role { OrganizationId = orgA.OrganizationId, Name = "Manager", IsActive = true, CreatedAtUtc = TestData.Now };
        var inactiveRole = new Role { OrganizationId = orgA.OrganizationId, Name = "Retired", IsActive = false, CreatedAtUtc = TestData.Now };
        ctx.AddRange(systemRoleInstance, customManager, inactiveRole);
        await ctx.SaveChangesAsync();

        ctx.Add(new RolePermission { RoleId = customManager.RoleId, PermissionId = orderManage.PermissionId });
        await ctx.SaveChangesAsync();

        var users = new Dictionary<string, User>();
        foreach (var label in new[] { "sys", "custom", "none", "inactive", "removed", "inactiverole", "orgb" })
        {
            var user = new User
            {
                FirstName = label,
                Email = $"{label}@perm.example.com",
                PasswordHash = "x",
                IsActive = true,
                CreatedAtUtc = TestData.Now
            };
            users[label] = user;
            ctx.Add(user);
        }
        await ctx.SaveChangesAsync();

        OrganizationUser Member(Organization org, string label, bool isActive = true, bool removed = false) => new()
        {
            OrganizationId = org.OrganizationId,
            UserId = users[label].UserId,
            IsActive = isActive,
            RemovedAtUtc = removed ? TestData.Now : null,
            JoinedAtUtc = TestData.Now
        };

        var mSystem = Member(orgA, "sys");
        var mCustom = Member(orgA, "custom");
        var mNone = Member(orgA, "none");
        var mInactive = Member(orgA, "inactive", isActive: false);
        var mRemoved = Member(orgA, "removed", removed: true);
        var mInactiveRole = Member(orgA, "inactiverole");
        var mB = Member(orgB, "orgb");
        ctx.AddRange(mSystem, mCustom, mNone, mInactive, mRemoved, mInactiveRole, mB);
        await ctx.SaveChangesAsync();

        world.SystemUserMembershipId = mSystem.OrganizationUserId;
        world.CustomUserMembershipId = mCustom.OrganizationUserId;
        world.NoRoleMembershipId = mNone.OrganizationUserId;
        world.InactiveMembershipId = mInactive.OrganizationUserId;
        world.RemovedMembershipId = mRemoved.OrganizationUserId;
        world.InactiveRoleMembershipId = mInactiveRole.OrganizationUserId;
        world.OrgBMembershipId = mB.OrganizationUserId;

        // Inactive/removed memberships ARE granted a role, to prove the membership state is what denies them.
        ctx.AddRange(
            new UserRole { OrganizationId = orgA.OrganizationId, OrganizationUserId = mSystem.OrganizationUserId, RoleId = systemRoleInstance.RoleId, AssignedAtUtc = TestData.Now },
            new UserRole { OrganizationId = orgA.OrganizationId, OrganizationUserId = mCustom.OrganizationUserId, RoleId = customManager.RoleId, AssignedAtUtc = TestData.Now },
            new UserRole { OrganizationId = orgA.OrganizationId, OrganizationUserId = mInactive.OrganizationUserId, RoleId = customManager.RoleId, AssignedAtUtc = TestData.Now },
            new UserRole { OrganizationId = orgA.OrganizationId, OrganizationUserId = mRemoved.OrganizationUserId, RoleId = customManager.RoleId, AssignedAtUtc = TestData.Now },
            new UserRole { OrganizationId = orgA.OrganizationId, OrganizationUserId = mInactiveRole.OrganizationUserId, RoleId = inactiveRole.RoleId, AssignedAtUtc = TestData.Now });
        await ctx.SaveChangesAsync();

        return world;
    }
}
