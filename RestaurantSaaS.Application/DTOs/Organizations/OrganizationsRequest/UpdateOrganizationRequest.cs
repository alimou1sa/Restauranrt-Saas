using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace RestaurantSaaS.Application.DTOs.Organizations.OrganizationsRequest;

// Slug is intentionally excluded: it is treated as an immutable identifier
// once the Organization is created (used as a stable external reference).
public class UpdateOrganizationRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = null!;

    [MaxLength(320), EmailAddress]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    public bool IsActive { get; set; }
}