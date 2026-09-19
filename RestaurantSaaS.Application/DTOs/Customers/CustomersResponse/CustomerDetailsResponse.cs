using System;

namespace RestaurantSaaS.Application.DTOs.Customers.CustomersResponse;

// Full shape for a single customer's profile view (also used after
// Create/Update). OrganizationId is dropped (route-scoped context).
public class CustomerDetailsResponse
{
    public int CustomerId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}