namespace RestaurantSaaS.Application.DTOs.Customers.CustomersResponse;

// Lean shape for a customers list/search table. OrganizationId is dropped
// (route-scoped context). Notes and timestamps are left out - internal
// staff notes and audit dates are not something a list row needs to show.
public class CustomerListResponse
{
    public int CustomerId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }
}
