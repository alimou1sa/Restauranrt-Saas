using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Products.ProductsRequest;
    using global::RestaurantSaaS.Application.InterfacesService;
    using global::RestaurantSaaS.Infrastructure;
    using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs.Products.ProductsResponse;
namespace RestaurantSaaS.Application.Services
{



    public class ProductService : IProductService
    {
        private readonly IAppDbContext _context;

        public ProductService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductDetailsResponse> CreateAsync(int categoryId, CreateProductRequest request)
        {

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category is null)
                throw new KeyNotFoundException($"Category {categoryId} was not found.");

            var nameExists = await _context.Products
                .AnyAsync(p => p.CategoryId == categoryId && p.Name == request.Name);

            if (nameExists)
                throw new InvalidOperationException($"A product named '{request.Name}' already exists in this category.");

            var product = new Product
            {
                CategoryId = categoryId,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                ImageUrl = request.ImageUrl,
                DisplayOrder = request.DisplayOrder,
                IsAvailable = true,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return ToDetailsResponse(product, category.Name);
        }

        public async Task<ProductDetailsResponse?> GetByIdAsync(int productId)
        {

            var response = await _context.Products
                .AsNoTracking()
                .Where(p => p.ProductId == productId)
                .Select(p => new ProductDetailsResponse
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    DisplayOrder = p.DisplayOrder,
                    IsAvailable = p.IsAvailable,
                    IsActive = p.IsActive,
                    CreatedAtUtc = p.CreatedAtUtc,
                    UpdatedAtUtc =  p.UpdatedAtUtc

                })
                .FirstOrDefaultAsync();

            return response;
        }

        public async Task<List<ProductListResponse>> GetAllByBranchAsync(int branchId)
        {
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.Category.Menu.BranchId == branchId)
                .Select(p => new ProductListResponse
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    IsAvailable = p.IsAvailable,
                    DisplayOrder = p.DisplayOrder
                })
                .ToListAsync();

            return products .ToList();
        }

        public async Task<ProductDetailsResponse?> UpdateAsync(int productId, UpdateProductRequest request)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product is null)
                return null;

            var nameExists = await _context.Products
                .AnyAsync(p => p.CategoryId == product.CategoryId
                            && p.Name == request.Name
                            && p.ProductId != productId);

            if (nameExists)
                throw new InvalidOperationException($"A product named '{request.Name}' already exists in this category.");

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.ImageUrl = request.ImageUrl;
            product.DisplayOrder = request.DisplayOrder;
            product.IsActive = request.IsActive;
            product.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ToDetailsResponse(product, product.Category.Name);
        }

        public async Task<ProductDetailsResponse?> UpdateAvailabilityAsync(int productId, UpdateProductAvailabilityRequest request)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product is null)
                return null;

            product.IsAvailable = request.IsAvailable;
            product.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ToDetailsResponse(product, product.Category.Name);
        }

        public async Task<bool> DeleteAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product is null)
                return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }


        private static ProductDetailsResponse ToDetailsResponse(Product product, string categoryName)
        {
            return new ProductDetailsResponse
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                CategoryId = product.CategoryId,
                CategoryName = categoryName,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                DisplayOrder = product.DisplayOrder,
                IsAvailable = product.IsAvailable,
                IsActive = product.IsActive,
                CreatedAtUtc = product.CreatedAtUtc,
                UpdatedAtUtc = product.UpdatedAtUtc
            };
        }
    }
}
