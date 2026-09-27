using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs.Organizations.OrganizationsRequest;
using RestaurantSaaS.Application.InterfacesService;
using RestaurantSaaS.Domain.Common;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace RestaurantSaaS.Application.Services
{

    public class OrganizationService : IOrganizationService
    {

        private readonly IAppDbContext _context;
        private readonly ICurrentTenant _currentTenant;
        public OrganizationService(IAppDbContext context, ICurrentTenant currentTenant)
        {
            _context = context;
            _currentTenant = currentTenant;
        }

        public async Task<OrganizationResponse> CreateAsync(CreateOrganizationRequest request)
        {
            var slugExists = await _context.Organizations
                .AnyAsync(o => o.Slug == request.Slug);

            if (slugExists)
                throw new InvalidOperationException($"An organization with slug '{request.Slug}' already exists.");

            var organization = new Organization
            {
                Name = request.Name,
                Slug = request.Slug,
                Email = request.Email,
                Phone = request.Phone,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();

            return ToResponse(organization);
        }

        public async Task<OrganizationResponse?> GetAsync()
        {
            var organizationId = _currentTenant.OrganizationId ?? throw new UnauthorizedAccessException("No organization context.");

            var organization = await _context.Organizations
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrganizationId == organizationId);

            return organization is null ? null : ToResponse(organization);
        }

        public async Task<List<OrganizationResponse>> GetAllAsync()
        {
            var organizations = await _context.Organizations
                .AsNoTracking()
                .ToListAsync();
        
            return   organizations.Select(ToResponse).ToList();
        }

        public async Task<OrganizationResponse?> UpdateAsync(UpdateOrganizationRequest request)
        {
            var organizationId = _currentTenant.OrganizationId ?? throw new UnauthorizedAccessException("No organization context.");
            var organization = await _context.Organizations.FindAsync(organizationId);
            if (organization is null) return null;


            organization.Name = request.Name;
            organization.Email = request.Email;
            organization.Phone = request.Phone;
            organization.IsActive = request.IsActive;
            organization.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ToResponse(organization);
        }

        public async Task<bool> DeleteAsync()
        {
            var organizationId = _currentTenant.OrganizationId ?? throw new UnauthorizedAccessException("No organization context.");
            var organization = await _context.Organizations.FindAsync(organizationId);
            if (organization is null)
                return false;

            _context.Organizations.Remove(organization);
            await _context.SaveChangesAsync();

            return true;
        }

        private static OrganizationResponse ToResponse(Organization organization)
        {
            return new OrganizationResponse
            {
                OrganizationId = organization.OrganizationId,
                Name = organization.Name,
                Slug = organization.Slug,
                Email = organization.Email,
                Phone = organization.Phone,
                IsActive = organization.IsActive,
                CreatedAtUtc = organization.CreatedAtUtc,
                UpdatedAtUtc = organization.UpdatedAtUtc
            };
        }
 
    }
}
