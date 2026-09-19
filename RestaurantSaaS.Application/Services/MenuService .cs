using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Menus.MenusRequest;
    using global::RestaurantSaaS.Application.DTOs.Menus.MenusRespose;
    using global::RestaurantSaaS.Application.InterfacesService;
    using global::RestaurantSaaS.Infrastructure;
    using Microsoft.EntityFrameworkCore;

namespace RestaurantSaaS.Application.Services
{


    public class MenuService : IMenuService
    {
        private readonly IAppDbContext _context;

        public MenuService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<MenuResponse> CreateAsync(int branchId, CreateMenuRequest request)
        {
            var branchExists = await _context.Branches
                .AnyAsync(b => b.BranchId == branchId);

            if (!branchExists)
                throw new KeyNotFoundException($"Branch {branchId} was not found.");

            var nameExists = await _context.Menus
                .AnyAsync(m => m.BranchId == branchId && m.Name == request.Name);

            if (nameExists)
                throw new InvalidOperationException($"A menu named '{request.Name}' already exists in this branch.");

            var menu = new Menu
            {
                BranchId = branchId,
                Name = request.Name,
                Description = request.Description,
                IsPublished = false,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            return ToResponse(menu);
        }

        public async Task<MenuResponse?> GetByIdAsync(int menuId)
        {
            var menu = await _context.Menus
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MenuId == menuId);

            return menu is null ? null : ToResponse(menu);
        }


        public async Task<List<MenuResponse>> GetAllByBranchAsync(int branchId)
        {
            var menus = await _context.Menus
                .AsNoTracking()
                .Where(m => m.BranchId == branchId)
                .ToListAsync();

            return menus.Select(ToResponse).ToList();
        }

        public async Task<MenuResponse?> UpdateAsync(int menuId, UpdateMenuRequest request)
        {
            var menu = await _context.Menus.FindAsync(menuId);
            if (menu is null)
                return null;

            var nameExists = await _context.Menus
                .AnyAsync(m => m.BranchId == menu.BranchId
                            && m.Name == request.Name
                            && m.MenuId != menuId);

            if (nameExists)
                throw new InvalidOperationException($"A menu named '{request.Name}' already exists in this branch.");

            menu.Name = request.Name;
            menu.Description = request.Description;
            menu.IsPublished = request.IsPublished;
            menu.IsActive = request.IsActive;
            menu.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ToResponse(menu);
        }

        public async Task<bool> DeleteAsync(int menuId)
        {
            var menu = await _context.Menus.FindAsync(menuId);
            if (menu is null)
                return false;

            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync();

            return true;
        }

        private static MenuResponse ToResponse(Menu menu)
        {
            return new MenuResponse
            {
                MenuId = menu.MenuId,
                Name = menu.Name,
                Description = menu.Description,
                IsPublished = menu.IsPublished,
                IsActive = menu.IsActive,
                CreatedAtUtc = menu.CreatedAtUtc,
                UpdatedAtUtc = menu.UpdatedAtUtc
            };
        }
    }
}
