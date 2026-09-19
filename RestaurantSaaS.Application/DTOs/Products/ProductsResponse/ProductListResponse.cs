namespace RestaurantSaaS.Application.DTOs.Products.ProductsResponse;

public class ProductListResponse
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; }

    public int DisplayOrder { get; set; }
}