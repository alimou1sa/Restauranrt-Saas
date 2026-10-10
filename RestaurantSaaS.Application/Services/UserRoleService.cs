    using global::RestaurantSaaS.Application.DTOs.UserRoles.RoleRequest;
    using global::RestaurantSaaS.Application.DTOs.UserRoles.RoleResponse;
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



    public class UserRoleService : IUserRoleService
    {
        private readonly IAppDbContext _context;
        private readonly IPermissionService _permissionService; 

            public UserRoleService(IAppDbContext context, IPermissionService permissionService)
            {
                _context = context;
                _permissionService = permissionService;
            }



        public async Task<UserRoleResponse> AssignAsync(int organizationUserId, AssignRoleRequest request)
        {
            var organizationUser = await _context.OrganizationUsers
                .FirstOrDefaultAsync(ou => ou.OrganizationUserId == organizationUserId);

            if (organizationUser is null)
                throw new KeyNotFoundException($"Organization member {organizationUserId} was not found.");

            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleId == request.RoleId);

            if (role is null)
                throw new KeyNotFoundException($"Role {request.RoleId} was not found.");

            if (role.OrganizationId != organizationUser.OrganizationId)
                throw new InvalidOperationException("This role does not belong to the member's organization.");

            var alreadyAssigned = await _context.UserRoles
                .AnyAsync(ur => ur.OrganizationUserId == organizationUserId && ur.RoleId == request.RoleId);

            if (alreadyAssigned)
                throw new InvalidOperationException("This role is already assigned to this member.");

            var userRole = new UserRole
            {
                OrganizationUserId = organizationUserId,
                RoleId = request.RoleId,
                OrganizationId = organizationUser.OrganizationId,
                AssignedAtUtc = DateTime.UtcNow
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
            _permissionService.InvalidateCache(organizationUserId);
            return ToResponse(userRole, role.Name);
        }
        /*
        public async Task<List<UserRoleResponse>> GetAllByOrganizationUserAsync(int organizationUserId)
        {
            return await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.OrganizationUserId == organizationUserId)
                .Select(ur => new UserRoleResponse
                {
                    UserRoleId = ur.UserRoleId,
                    RoleId = ur.RoleId,
                    RoleName = ur.Role.Name,
                    AssignedAtUtc = ur.AssignedAtUtc
                })
                .ToListAsync();
        }
        */


        public async Task<List<UserRoleResponse>>
            GetAllByOrganizationUserAsync(int organizationUserId)
        {
            return await _context.UserRoles
                .AsNoTracking()
                .Where(ur =>ur.OrganizationUserId == organizationUserId)
                .Select(ur => new UserRoleResponse
                {
                    UserRoleId = ur.UserRoleId,
                    RoleId = ur.RoleId,

                    RoleName = ur.Role.SystemRole != null
                        ? ur.Role.SystemRole.Name
                        : ur.Role.Name!,

                    AssignedAtUtc = ur.AssignedAtUtc
                })
                .OrderBy(ur => ur.RoleName)
                .ToListAsync();
        }





        public async Task<bool> RemoveAsync(int userRoleId)
        {
            var userRole = await _context.UserRoles
      .FirstOrDefaultAsync(ur => ur.UserRoleId == userRoleId);
            if (userRole is null)
                return false;
          

            var organizationUserId = userRole.OrganizationUserId;   

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();

            _permissionService.InvalidateCache(organizationUserId);


            return true;
        }

        private static UserRoleResponse ToResponse(UserRole userRole, string roleName)
        {
            return new UserRoleResponse
            {
                UserRoleId = userRole.UserRoleId,
                RoleId = userRole.RoleId,
                RoleName = roleName,
                AssignedAtUtc = userRole.AssignedAtUtc

            };
        }
    }
}
