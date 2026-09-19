using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace RestaurantSaaS.Application.DTOs.OrganizationUsers.OrganizationUserRequest;

// OrganizationId is resolved from the route/tenant context, not the body.
// BranchId is optional: null means an org-wide member (not tied to one branch).
public class CreateOrganizationUserRequest
{
    [Required]
    public int UserId { get; set; }

    public int? BranchId { get; set; }
}



