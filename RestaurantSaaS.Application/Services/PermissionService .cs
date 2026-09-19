using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Permissions.PermissionRequest;
    using global::RestaurantSaaS.Application.DTOs.Permissions.PermissionResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
    using global::RestaurantSaaS.Infrastructure;
    using Microsoft.EntityFrameworkCore;
namespace RestaurantSaaS.Application.Services
{


    public class PermissionService : IPermissionService
    {
        private readonly IAppDbContext _context;

        public PermissionService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<PermissionResponse> CreateAsync(CreatePermissionRequest request)
        {
            var codeExists = await _context.Permissions
                .AnyAsync(p => p.Code == request.Code);

            if (codeExists)
                throw new InvalidOperationException($"A permission with code '{request.Code}' already exists.");

            var permission = new Permission
            {
                Code = request.Code,
                Name = request.Name,
                Description = request.Description
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            return ToResponse(permission);
        }

        public async Task<PermissionResponse?> GetByIdAsync(int permissionId)
        {
            var permission = await _context.Permissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PermissionId == permissionId);

            return permission is null ? null : ToResponse(permission);
        }

        public async Task<List<PermissionResponse>> GetAllAsync()
        {
            var permissions = await _context.Permissions
                .AsNoTracking()
                .ToListAsync();

            return permissions.Select(ToResponse).ToList();
        }

        public async Task<PermissionResponse?> UpdateAsync(int permissionId, UpdatePermissionRequest request)
        {
            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission is null)
                return null;

            permission.Name = request.Name;
            permission.Description = request.Description;

            await _context.SaveChangesAsync();

            return ToResponse(permission);
        }

        public async Task<bool> DeleteAsync(int permissionId)
        {
            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission is null)
                return false;

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();

            return true;
        }

        private static PermissionResponse ToResponse(Permission permission)
        {
            return new PermissionResponse
            {
                PermissionId = permission.PermissionId,
                Code = permission.Code,
                Name = permission.Name,
                Description = permission.Description
            };
        }
    }
}
