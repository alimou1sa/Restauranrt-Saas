using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestaurantSaaS.Application.DTOs.OrganizationUsers.OrganizationUserRequest;
using RestaurantSaaS.Application.DTOs.OrganizationUsers.OrganizationUserResponse;
using RestaurantSaaS.Application.InterfacesService;
using RestaurantSaaS.Infrastructure;
    using Microsoft.EntityFrameworkCore;
namespace RestaurantSaaS.Application.Services
{

    public class OrganizationUserService : IOrganizationUserService
    {
        private readonly IAppDbContext _context;

        public OrganizationUserService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<OrganizationUserResponse> CreateAsync(int organizationId, CreateOrganizationUserRequest request)
        {
            var organizationExists = await _context.Organizations
                .AnyAsync(o => o.OrganizationId == organizationId);

            if (!organizationExists)
                throw new KeyNotFoundException($"Organization {organizationId} was not found.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == request.UserId);

            if (user is null)
                throw new KeyNotFoundException($"User {request.UserId} was not found.");

            if (request.BranchId.HasValue)
            {
                var branchBelongsToOrg = await _context.Branches
                    .AnyAsync(b => b.BranchId == request.BranchId.Value && b.OrganizationId == organizationId);

                if (!branchBelongsToOrg)
                    throw new KeyNotFoundException($"Branch {request.BranchId} does not belong to this organization.");
            }

            var alreadyMember = await _context.OrganizationUsers
                .AnyAsync(ou => ou.OrganizationId == organizationId && ou.UserId == request.UserId);

            if (alreadyMember)
                throw new InvalidOperationException("This user is already a member of this organization.");

            var organizationUser = new OrganizationUser
            {
                OrganizationId = organizationId,
                UserId = request.UserId,
                BranchId = request.BranchId,
                IsActive = true,
                JoinedAtUtc = DateTime.UtcNow
            };

            _context.OrganizationUsers.Add(organizationUser);
            await _context.SaveChangesAsync();

            var branchName = request.BranchId.HasValue
                ? await _context.Branches.Where(b => b.BranchId == request.BranchId).Select(b => b.Name).FirstOrDefaultAsync()
                : null;

            return ToResponse(organizationUser, FullName(user.FirstName, user.LastName), user.Email, branchName);
        }

        public async Task<OrganizationUserResponse?> GetByIdAsync(int organizationUserId)
        {
            return await Projection()
                .FirstOrDefaultAsync(ou => ou.OrganizationUserId == organizationUserId);
        }

        public async Task<List<OrganizationUserResponse>> GetAllByOrganizationAsync(int organizationId)
        {
            return await Projection()
                .Where(ou => ou.OrganizationId == organizationId)
                .ToListAsync();
        }

        public async Task<OrganizationUserResponse?> UpdateAsync(int organizationUserId, UpdateOrganizationUserRequest request)
        {
            var organizationUser = await _context.OrganizationUsers.FindAsync(organizationUserId);
            if (organizationUser is null)
                return null;

            if (request.BranchId.HasValue)
            {
                var branchBelongsToOrg = await _context.Branches
                    .AnyAsync(b => b.BranchId == request.BranchId.Value && b.OrganizationId == organizationUser.OrganizationId);

                if (!branchBelongsToOrg)
                    throw new KeyNotFoundException($"Branch {request.BranchId} does not belong to this organization.");
            }

            organizationUser.BranchId = request.BranchId;
            organizationUser.IsActive = request.IsActive;

            await _context.SaveChangesAsync();

            return await GetByIdAsync(organizationUserId);
        }

        public async Task<bool> DeleteAsync(int organizationUserId)
        {
            var organizationUser = await _context.OrganizationUsers.FindAsync(organizationUserId);
            if (organizationUser is null)
                return false;

            organizationUser.IsActive = false;
            organizationUser.RemovedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        private IQueryable<OrganizationUserResponse> Projection()
        {
            return _context.OrganizationUsers
                .AsNoTracking()
                .Select(ou => new OrganizationUserResponse
                {
                    OrganizationUserId = ou.OrganizationUserId,
                    UserId = ou.UserId,
                    UserFullName = ou.User.LastName == null ? ou.User.FirstName : ou.User.FirstName + " " + ou.User.LastName,
                    UserEmail = ou.User.Email,
                    BranchId = ou.BranchId,
                    BranchName = ou.Branch != null ? ou.Branch.Name : null,
                    IsActive = ou.IsActive,
                    JoinedAtUtc = ou.JoinedAtUtc,
                    RemovedAtUtc = ou.RemovedAtUtc
                });
        }

        private static string FullName(string firstName, string? lastName) =>
            lastName is null ? firstName : $"{firstName} {lastName}";

        private static OrganizationUserResponse ToResponse(OrganizationUser organizationUser, string userFullName, string userEmail, string? branchName)
        {
            return new OrganizationUserResponse
            {
                OrganizationUserId = organizationUser.OrganizationUserId,
                UserId = organizationUser.UserId,
                UserFullName = userFullName,
                UserEmail = userEmail,
                BranchId = organizationUser.BranchId,
                BranchName = branchName,
                IsActive = organizationUser.IsActive,
                JoinedAtUtc = organizationUser.JoinedAtUtc,
                RemovedAtUtc = organizationUser.RemovedAtUtc
            };
        }
    }
}
