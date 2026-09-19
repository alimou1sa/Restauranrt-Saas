using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.RolePermissions.RolePermissionsRequest;
    using global::RestaurantSaaS.Application.DTOs.RolePermissions.RolePermissionsResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
    using global::RestaurantSaaS.Infrastructure;
    using Microsoft.EntityFrameworkCore;
namespace RestaurantSaaS.Application.Services
{


    public class RolePermissionService : IRolePermissionService
    {
        private readonly IAppDbContext _context;

        public RolePermissionService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<RolePermissionResponse> AssignAsync(int roleId, AssignPermissionRequest request)
        {
            var roleExists = await _context.Roles.AnyAsync(r => r.RoleId == roleId);
            if (!roleExists)
                throw new KeyNotFoundException($"Role {roleId} was not found.");

            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.PermissionId == request.PermissionId);

            if (permission is null)
                throw new KeyNotFoundException($"Permission {request.PermissionId} was not found.");

            var alreadyAssigned = await _context.RolePermissions
                .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == request.PermissionId);

            if (alreadyAssigned)
                throw new InvalidOperationException("This permission is already assigned to this role.");

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = request.PermissionId
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();

            return ToResponse(rolePermission, permission.Code, permission.Name);
        }

  
        public async Task<List<RolePermissionResponse>> GetAllByRoleAsync(int roleId)
        {
            return await _context.RolePermissions
                .AsNoTracking()
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => new RolePermissionResponse
                {
                    RolePermissionId = rp.RolePermissionId,
                    PermissionId = rp.PermissionId,
                    PermissionCode = rp.Permission.Code,
                    PermissionName = rp.Permission.Name
                })
                .ToListAsync();
        }

        public async Task<bool> RemoveAsync(int rolePermissionId)
        {
            var rolePermission = await _context.RolePermissions.FindAsync(rolePermissionId);
            if (rolePermission is null)
                return false;

            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();

            return true;
        }

        private static RolePermissionResponse ToResponse(RolePermission rolePermission, string code, string name)
        {
            return new RolePermissionResponse
            {
                RolePermissionId = rolePermission.RolePermissionId,
                PermissionId = rolePermission.PermissionId,
                PermissionCode = code,
                PermissionName = name
            };
        }
    }
}
