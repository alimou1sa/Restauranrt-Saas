using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace RestaurantSaaS.Application.DTOs.Permissions.PermissionRequest;

// Note: Permissions form a global, platform-level catalog (not scoped to
// an Organization). In practice this endpoint should be restricted to a
// system/platform-admin role, not exposed to regular tenant users.
public class CreatePermissionRequest
{
    [Required, MaxLength(100)]
    public string Code { get; set; } = null!;

    [Required, MaxLength(150)]
    public string Name { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }
}