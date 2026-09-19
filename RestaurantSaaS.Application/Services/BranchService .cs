using global::RestaurantSaaS.Application.DTOs.Branches.BranchRequest;
using global::RestaurantSaaS.Application.InterfacesService;
using global::RestaurantSaaS.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace RestaurantSaaS.Application.Services
{

    public class BranchService : IBranchService
    {
        private readonly IAppDbContext _context;

        public BranchService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<BranchResponse> CreateAsync(int organizationId, CreateBranchRequest request)
        {
            var organizationExists = await _context.Organizations
                .AnyAsync(o => o.OrganizationId == organizationId);

            if (!organizationExists)
                throw new KeyNotFoundException($"Organization {organizationId} was not found.");

            var nameExists = await _context.Branches
                .AnyAsync(b => b.OrganizationId == organizationId && b.Name == request.Name);

            if (nameExists)
                throw new InvalidOperationException($"A branch named '{request.Name}' already exists in this organization.");

            var branch = new Branch
            {
                OrganizationId = organizationId,
                Name = request.Name,
                Address = request.Address,
                Phone = request.Phone,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            return ToResponse(branch);
        }

        public async Task<BranchResponse?> GetByIdAsync(int branchId)
        {
            var branch = await _context.Branches
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BranchId == branchId);

            return branch is null ? null : ToResponse(branch);
        }

        public async Task<List<BranchResponse>> GetAllByOrganizationAsync(int organizationId)
        {
            var branches = await _context.Branches
                .AsNoTracking()
                .Where(b => b.OrganizationId == organizationId)
                .ToListAsync();

            return branches.Select(ToResponse).ToList();
        }

        public async Task<BranchResponse?> UpdateAsync(int branchId, UpdateBranchRequest request)
        {
            var branch = await _context.Branches.FindAsync(branchId);
            if (branch is null)
                return null;

            var nameExists = await _context.Branches
                .AnyAsync(b => b.OrganizationId == branch.OrganizationId && b.Name == request.Name
                             && b.BranchId != branchId);

            if (nameExists)
                throw new InvalidOperationException($"A branch named '{request.Name}' already exists in this organization.");

            branch.Name = request.Name;
            branch.Address = request.Address;
            branch.Phone = request.Phone;
            branch.IsActive = request.IsActive;
            branch.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ToResponse(branch);
        }

        public async Task<bool> DeleteAsync(int branchId)
        {
            var branch = await _context.Branches.FindAsync(branchId);
            if (branch is null)
                return false;

            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();

            return true;
        }

        private static BranchResponse ToResponse(Branch branch)
        {
            return new BranchResponse
            {
                BranchId = branch.BranchId,
                Name = branch.Name,
                Address = branch.Address,
                Phone = branch.Phone,
                IsActive = branch.IsActive,
                CreatedAtUtc = branch.CreatedAtUtc,
                UpdatedAtUtc = branch.UpdatedAtUtc
            };
        }
    }
}
