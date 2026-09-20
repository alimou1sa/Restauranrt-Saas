    using global::RestaurantSaaS.Application.DTOs.Permissions.PermissionRequest;
    using global::RestaurantSaaS.Application.DTOs.Permissions.PermissionResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
    using global::RestaurantSaaS.Infrastructure;
    using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RestaurantSaaS.Application.Services
{


    public class PermissionService : IPermissionService
    {

        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);
        private const string CacheKeyPrefix = "perms:";

        private readonly IAppDbContext _context;
        private readonly IMemoryCache _cache;

        public PermissionService(IAppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<HashSet<string>> GetPermissionsAsync(int organizationUserId)
        {
            var cacheKey = CacheKeyPrefix + organizationUserId;

            if (_cache.TryGetValue(cacheKey, out HashSet<string>? cached) && cached is not null)
                return cached;

            var permissions = await LoadPermissionsAsync(organizationUserId);

            _cache.Set(cacheKey, permissions, CacheTtl);
            return permissions;
        }

        public void InvalidateCache(int organizationUserId)
        {
            _cache.Remove(CacheKeyPrefix + organizationUserId);
        }

        private async Task<HashSet<string>> LoadPermissionsAsync(int organizationUserId)
        {

            var membershipActive = await _context.OrganizationUsers
                .AsNoTracking()
                .AnyAsync(ou => ou.OrganizationUserId == organizationUserId &&
                    ou.IsActive &&ou.RemovedAtUtc == null);

            if (!membershipActive)
                return new HashSet<string>();

            var codes = await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.OrganizationUserId == organizationUserId && ur.Role.IsActive)
                .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Code))
                .Distinct().ToListAsync();

            return codes.ToHashSet();
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
