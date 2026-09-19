using System;

namespace RestaurantSaaS.Application.DTOs.Organizations.OrganizationsRequest;

// No List/Details split: Organization has no foreign keys and no heavy
// fields to trim - the same shape genuinely fits both a tenants list and
// a single organization view.
public class OrganizationResponse
{
    public int OrganizationId { get; set; } // was incorrectly `int` - fixed to match the entity/service (bigint)

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}