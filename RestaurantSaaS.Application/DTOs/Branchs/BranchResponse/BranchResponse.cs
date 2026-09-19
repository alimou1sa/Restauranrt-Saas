using System;

namespace RestaurantSaaS.Application.DTOs.Branches.BranchRequest;

// OrganizationId removed: Branch is always fetched within an already-known
// Organization context (route-scoped), so repeating it on every row is
// redundant. No List/Details split: all remaining fields are lightweight
// and equally relevant in a list row or a detail/edit view.
public class BranchResponse
{
    public int BranchId { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}