using System;

namespace RestaurantSaaS.Application.DTOs.Menus.MenusRespose;

// BranchId removed: Menu is always fetched within an already-known Branch
// context (route-scoped). No List/Details split: all fields are lightweight
// scalars equally relevant in a list row or an edit view.
public class MenuResponse
{
    public int MenuId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsPublished { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}