using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs.Organizations.OrganizationsRequest;
using RestaurantSaaS.Application.InterfacesService;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.Services
{
    public class PlatformOrganizationService : IPlatformOrganizationService
    {
        private readonly IAppDbContext _context;
        public PlatformOrganizationService(IAppDbContext context) => _context = context;

        public async Task<List<OrganizationResponse>> GetAllAsync()
        {
            var organizations = await _context.Organizations.AsNoTracking().ToListAsync();
            return organizations.Select(ToResponse).ToList();
        }

        public async Task<OrganizationResponse?> GetByIdAsync(int organizationId)
        {
            var organization = await _context.Organizations.AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrganizationId == organizationId);
            return organization is null ? null : ToResponse(organization);
        }

        public async Task<OrganizationResponse?> SetActiveAsync(int organizationId, bool isActive)
        {
            var organization = await _context.Organizations.FindAsync(organizationId);
            if (organization is null) return null;

            organization.IsActive = isActive;
            organization.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return ToResponse(organization);
        }

        private static OrganizationResponse ToResponse(Organization o) => new()
        {
            OrganizationId = o.OrganizationId,
            Name = o.Name,
            Slug = o.Slug,
            Email = o.Email,
            Phone = o.Phone,
            IsActive = o.IsActive,
            CreatedAtUtc = o.CreatedAtUtc,
            UpdatedAtUtc = o.UpdatedAtUtc
        };
    }
}
