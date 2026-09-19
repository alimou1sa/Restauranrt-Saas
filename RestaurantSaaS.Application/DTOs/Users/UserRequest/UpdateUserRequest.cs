using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace RestaurantSaaS.Application.DTOs.Users.UserRequest;

public class UpdateUserRequest
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = null!;

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }
}