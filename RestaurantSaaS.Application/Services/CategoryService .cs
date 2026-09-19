using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Categories.CategoriesRequest;
    using global::RestaurantSaaS.Application.DTOs.Categories.CategoriesResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
    using global::RestaurantSaaS.Infrastructure;
    using Microsoft.EntityFrameworkCore;

namespace RestaurantSaaS.Application.Services
{

    public class CategoryService : ICategoryService
    {
        private readonly IAppDbContext _context;

        public CategoryService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryResponse> CreateAsync(int menuId, CreateCategoryRequest request)
        {
            var menuExists = await _context.Menus
                .AnyAsync(m => m.MenuId == menuId);

            if (!menuExists)
                throw new KeyNotFoundException($"Menu {menuId} was not found.");

            var nameExists = await _context.Categories
                .AnyAsync(c => c.MenuId == menuId && c.Name == request.Name);

            if (nameExists)
                throw new InvalidOperationException($"A category named '{request.Name}' already exists in this menu.");

            var category = new Category
            {
                MenuId = menuId,
                Name = request.Name,
                Description = request.Description,
                DisplayOrder = request.DisplayOrder,
                IsActive = true
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return ToResponse(category);
        }

        public async Task<CategoryResponse?> GetByIdAsync(int categoryId)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            return category is null ? null : ToResponse(category);
        }

        public async Task<List<CategoryResponse>> GetAllByMenuAsync(int menuId)
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.MenuId == menuId)
                .ToListAsync();

            return categories.Select(ToResponse).ToList();
        }

        public async Task<CategoryResponse?> UpdateAsync(int categoryId, UpdateCategoryRequest request)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category is null)
                return null;

            var nameExists = await _context.Categories
                .AnyAsync(c => c.MenuId == category.MenuId
                            && c.Name == request.Name
                            && c.CategoryId != categoryId);

            if (nameExists)
                throw new InvalidOperationException($"A category named '{request.Name}' already exists in this menu.");

            category.Name = request.Name;
            category.Description = request.Description;
            category.DisplayOrder = request.DisplayOrder;
            category.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return ToResponse(category);
        }

        public async Task<bool> DeleteAsync(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category is null)
                return false;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return true;
        }

        private static CategoryResponse ToResponse(Category category)
        {
            return new CategoryResponse
            {
                CategoryId = category.CategoryId,
      
                Name = category.Name,
                Description = category.Description,
                DisplayOrder = category.DisplayOrder,
                IsActive = category.IsActive
            };
        }
    }
}
