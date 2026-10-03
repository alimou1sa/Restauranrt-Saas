using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace RestaurantSaaS.Application.DTOs.OrganizationUsers.OrganizationUserRequest;


public class CreateOrganizationUserRequest
{
    [Required]
    public int UserId { get; set; }

    public int? BranchId { get; set; }
}



