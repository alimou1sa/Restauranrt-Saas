    using global::RestaurantSaaS.Application.DTOs.Roles.RoleRequest;
    using global::RestaurantSaaS.Application.DTOs.Roles.RoleResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
    using global::RestaurantSaaS.Infrastructure;
    using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.Services
{

    public class RoleService : IRoleService
    {
        private readonly IAppDbContext _context;

        public RoleService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<RoleResponse> CreateCustomAsync(int organizationId, CreateCustomRoleRequest request)
        {
            var organizationExists = await _context.Organizations
                .AnyAsync(o => o.OrganizationId == organizationId);

            if (!organizationExists)
                throw new KeyNotFoundException($"Organization {organizationId} was not found.");

            var nameExists = await _context.Roles
                .AnyAsync(r =>
                    r.OrganizationId == organizationId &&
                    r.SystemRoleId == null &&
                    r.Name == request.Name);

            if (nameExists)
                throw new InvalidOperationException($"A custom role named '{request.Name}' already exists.");

            var role = new Role
            {
                OrganizationId = organizationId,
                Name = request.Name,
                Description = request.Description,
                SystemRoleId = null,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Roles.Add(role);

            await _context.SaveChangesAsync();

            return await GetByIdAsync(role.RoleId)
                ?? throw new InvalidOperationException("Failed to retrieve the created role.");
        }

        public async Task<RoleResponse> AddSystemRoleAsync(int organizationId, int systemRoleId)
        {
            var organizationExists = await _context.Organizations
                .AnyAsync(o => o.OrganizationId == organizationId);

            if (!organizationExists)
                throw new KeyNotFoundException($"Organization {organizationId} was not found.");

            var systemRole = await _context.SystemRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(sr =>sr.SystemRoleId == systemRoleId);

            if (systemRole is null)
                throw new KeyNotFoundException($"System role {systemRoleId} was not found.");

            var alreadyExists = await _context.Roles
                .AnyAsync(r =>
                    r.OrganizationId == organizationId &&
                    r.SystemRoleId == systemRoleId);

            if (alreadyExists)
                throw new InvalidOperationException("This system role is already assigned to this organization.");

            var role = new Role
            {
                OrganizationId = organizationId,

                Name = systemRole.Name,
                Description = systemRole.Description,

                SystemRoleId = systemRole.SystemRoleId,

                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Roles.Add(role);

            await _context.SaveChangesAsync();

            return await GetByIdAsync(role.RoleId)
                ?? throw new InvalidOperationException(
                    "Failed to retrieve the created role.");
        }

        public async Task<RoleResponse?> GetByIdAsync(int roleId)
        {
     
            var role = await _context.Roles
    .AsNoTracking()
    .Where(r => r.RoleId == roleId)
    .Select(r => new RoleResponse
    {
        RoleId = r.RoleId,
        OrganizationId = r.OrganizationId,
        Name = r.Name!,
        Description = r.Description,
        SystemRoleId = r.SystemRoleId,
        SystemRoleName = r.SystemRole != null
            ? r.SystemRole.Name
            : null,
        IsActive = r.IsActive,
        CreatedAtUtc = r.CreatedAtUtc
    })
    .FirstOrDefaultAsync();

            return role ;
        }
        public async Task<List<RoleResponse>> GetAllByOrganizationAsync(int organizationId)
        {
            return await _context.Roles
                .AsNoTracking()
                .Where(r => r.OrganizationId == organizationId)
                .Select(r => new RoleResponse
                {
                    RoleId = r.RoleId,
                    OrganizationId = r.OrganizationId,
                    Name = r.Name!,
                    Description = r.Description,
                    SystemRoleId = r.SystemRoleId,
                    SystemRoleName = r.SystemRole != null
                        ? r.SystemRole.Name: null,
                    IsActive = r.IsActive,
                    CreatedAtUtc = r.CreatedAtUtc
                })
                .ToListAsync();
        }

        public async Task<RoleResponse?> UpdateCustomAsync(int roleId, UpdateCustomRoleRequest request)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleId == roleId);

            if (role is null)
                return null;



            if (role.SystemRoleId.HasValue)
                throw new InvalidOperationException("System roles cannot be modified.");

            var nameExists = await _context.Roles
                .AnyAsync(r =>
                    r.OrganizationId == role.OrganizationId &&
                    r.SystemRoleId == null &&
                    r.Name == request.Name &&
                    r.RoleId != roleId);

            if (nameExists)
                throw new InvalidOperationException($"A custom role named '{request.Name}' already exists.");

            role.Name = request.Name;
            role.Description = request.Description;
            role.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return ToResponse(role);
        }

        public async Task<bool> DeleteCustomAsync(int roleId)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleId == roleId);

            if (role is null)
                return false;

            if (role.SystemRoleId.HasValue)
                throw new InvalidOperationException(
                    "System roles cannot be deleted.");

            _context.Roles.Remove(role);

            await _context.SaveChangesAsync();

            return true;
        }

        private static RoleResponse ToResponse(Role role)
        {
            return new RoleResponse
            {
                RoleId = role.RoleId,
                OrganizationId = role.OrganizationId,
                Name = role.Name!,
                Description = role.Description,
                SystemRoleId = role.SystemRoleId,
                SystemRoleName = role.SystemRole?.Name,
                IsActive = role.IsActive,
                CreatedAtUtc = role.CreatedAtUtc
            };
        }
    }

}