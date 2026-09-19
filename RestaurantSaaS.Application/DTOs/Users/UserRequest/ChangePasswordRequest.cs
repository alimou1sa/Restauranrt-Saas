using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace RestaurantSaaS.Application.DTOs.Users.UserRequest;

public class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = null!;

    [Required, MinLength(8), MaxLength(100)]
    public string NewPassword { get; set; } = null!;
}
