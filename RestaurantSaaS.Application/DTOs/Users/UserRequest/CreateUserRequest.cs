using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace RestaurantSaaS.Application.DTOs.Users.UserRequest;

public class CreateUserRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [MaxLength(100)]
    public string? LastName { get; set; }

    [Required, MaxLength(320), EmailAddress]
    public string Email { get; set; } = null!;

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = null!;

    [MaxLength(30)]
    public string? Phone { get; set; }
}