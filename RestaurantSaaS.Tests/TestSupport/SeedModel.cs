namespace RestaurantSaaS.Tests.TestSupport;

/// <summary>Ids created for one branch (and its menu/category/product/order chain).</summary>
public sealed class BranchSeed
{
    public int BranchId { get; set; }
    public int MenuId { get; set; }
    public int CategoryId { get; set; }
    public int ProductId { get; set; }
    public int TableId { get; set; }
    public int OrderId { get; set; }
    public int OrderItemId { get; set; }
    public int PaymentId { get; set; }
    public int InventoryId { get; set; }
    public int InventoryTransactionId { get; set; }
}

/// <summary>Ids created for one organization.</summary>
public sealed class OrgSeed
{
    public int OrganizationId { get; set; }
    public int UserId { get; set; }
    public int OrganizationUserId { get; set; }
    public int RoleId { get; set; }
    public int CustomerId { get; set; }
    public int SubscriptionId { get; set; }
    public int UserRoleId { get; set; }

    public Dictionary<string, BranchSeed> Branches { get; } = new();

    public BranchSeed Branch(string key) => Branches[key];
}

/// <summary>Everything the seed created, referenced by id from the tests.</summary>
public sealed class SeededWorld
{
    public int PlanId { get; set; }
    public int PermissionId { get; set; }
    public int SystemRoleId { get; set; }

    public OrgSeed OrgA { get; set; } = new();
    public OrgSeed OrgB { get; set; } = new();
}
