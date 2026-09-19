using System;

namespace RestaurantSaaS.Application.DTOs.Categories.CategoriesResponse;

// MenuId removed: Category is always fetched within an already-known Menu
// context (route-scoped).
public class CategoryResponse
{
    public int CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}
