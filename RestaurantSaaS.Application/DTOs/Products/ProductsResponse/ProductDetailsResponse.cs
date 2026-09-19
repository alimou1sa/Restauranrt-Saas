using System;

namespace RestaurantSaaS.Application.DTOs.Products.ProductsResponse;


public class ProductDetailsResponse
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsAvailable { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}