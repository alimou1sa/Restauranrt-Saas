using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs.SystemRolePermissions.SystemRolePermissionsRequest;
using RestaurantSaaS.Application.DTOs.SystemRolePermissions.SystemRolePermissionsResponse;
using RestaurantSaaS.Application.InterfacesService;
using RestaurantSaaS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.Services
{
    public class SystemRolePermissionService : ISystemRolePermissionService
    {
        private readonly IAppDbContext _context;

        public SystemRolePermissionService(IAppDbContext context) => _context = context;

        public async Task<SystemRolePermissionResponse> AssignAsync(int systemRoleId, CreateSystemRolePermissionRequest request)
        {
            var systemRole = await _context.SystemRoles.FirstOrDefaultAsync(sr => sr.SystemRoleId == systemRoleId);
            if (systemRole is null)
                throw new KeyNotFoundException($"System role {systemRoleId} was not found.");

            var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.PermissionId == request.PermissionId);
            if (permission is null)
                throw new KeyNotFoundException($"Permission {request.PermissionId} was not found.");

            var alreadyAssigned = await _context.SystemRolePermissions
                .AnyAsync(srp => srp.SystemRoleId == systemRoleId && srp.PermissionId == request.PermissionId);
            if (alreadyAssigned)
                throw new InvalidOperationException("This permission is already assigned to this system role.");

            var srp2 = new SystemRolePermission { SystemRoleId = systemRoleId, PermissionId = request.PermissionId };
            _context.SystemRolePermissions.Add(srp2);
            await _context.SaveChangesAsync();

            return ToResponse(srp2, systemRole.Name, permission.Code, permission.Name);
        }

        public async Task<List<SystemRolePermissionResponse>> GetAllBySystemRoleAsync(int systemRoleId)
        {
            return await _context.SystemRolePermissions
                .AsNoTracking()
                .Where(srp => srp.SystemRoleId == systemRoleId)
                .Select(srp => new SystemRolePermissionResponse
                {
                    SystemRolePermissionId = srp.SystemRolePermissionId,
                    SystemRoleId = srp.SystemRoleId,
                    SystemRoleName = srp.SystemRole.Name,
                    PermissionId = srp.PermissionId,
                    PermissionCode = srp.Permission.Code,
                    PermissionName = srp.Permission.Name
                })
                .ToListAsync();
        }

        public async Task<bool> RemoveAsync(int systemRolePermissionId)
        {
            var srp = await _context.SystemRolePermissions.FindAsync(systemRolePermissionId);
            if (srp is null) return false;

            _context.SystemRolePermissions.Remove(srp);
            await _context.SaveChangesAsync();
            return true;
        }

        private static SystemRolePermissionResponse ToResponse(SystemRolePermission srp, string roleName, string code, string name)
        {
            return new SystemRolePermissionResponse
            {
                SystemRolePermissionId = srp.SystemRolePermissionId,
                SystemRoleId = srp.SystemRoleId,
                SystemRoleName = roleName,
                PermissionId = srp.PermissionId,
                PermissionCode = code,
                PermissionName = name
            };
        }
    }
}
