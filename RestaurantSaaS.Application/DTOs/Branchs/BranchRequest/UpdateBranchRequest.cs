using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace RestaurantSaaS.Application.DTOs.Branches.BranchRequest;

public class UpdateBranchRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = null!;

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    public bool IsActive { get; set; }
}