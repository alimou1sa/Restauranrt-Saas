using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace RestaurantSaaS.Application.DTOs.Permissions.PermissionRequest;

// Code is excluded: it is the stable identifier referenced by authorization
// checks in code, and should not change after creation.
public class UpdatePermissionRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }
}