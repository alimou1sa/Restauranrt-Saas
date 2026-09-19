using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using global::RestaurantSaaS.Application.DTOs.Customers.CustomersRequest;
    using global::RestaurantSaaS.Application.DTOs.Customers.CustomersResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
    using global::RestaurantSaaS.Infrastructure;
    using Microsoft.EntityFrameworkCore;

namespace RestaurantSaaS.Application.Services
{

    public class CustomerService : ICustomerService
    {
        private readonly IAppDbContext _context;

        public CustomerService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerDetailsResponse> CreateAsync(int organizationId, CreateCustomerRequest request)
        {
            var organizationExists = await _context.Organizations
                .AnyAsync(o => o.OrganizationId == organizationId);

            if (!organizationExists)
                throw new KeyNotFoundException($"Organization {organizationId} was not found.");

            var customer = new Customer
            {
                OrganizationId = organizationId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                Email = request.Email,
                Notes = request.Notes,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return ToDetailsResponse(customer);
        }

        public async Task<CustomerDetailsResponse?> GetByIdAsync(int customerId)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            return customer is null ? null : ToDetailsResponse(customer);
        }

        public async Task<List<CustomerListResponse>> GetAllByOrganizationAsync(int organizationId)
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .Where(c => c.OrganizationId == organizationId)
                .Select(c => new CustomerListResponse
                {
                    CustomerId = c.CustomerId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Phone = c.Phone,
                    Email = c.Email
                })
                .ToListAsync();

            return customers;
        }

        public async Task<CustomerDetailsResponse?> UpdateAsync(int customerId, UpdateCustomerRequest request)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer is null)
                return null;

            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.Phone = request.Phone;
            customer.Email = request.Email;
            customer.Notes = request.Notes;
            customer.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return ToDetailsResponse(customer);
        }

        public async Task<bool> DeleteAsync(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer is null)
                return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return true;
        }

        private static CustomerDetailsResponse ToDetailsResponse(Customer customer)
        {
            return new CustomerDetailsResponse
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Phone = customer.Phone,
                Email = customer.Email,
                Notes = customer.Notes,
                CreatedAtUtc = customer.CreatedAtUtc,
                UpdatedAtUtc = customer.UpdatedAtUtc
            };
        }
    }
}
