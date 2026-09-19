using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.OrganizationUsers.OrganizationUserRequest;


public class UpdateOrganizationUserRequest
{
    public int? BranchId { get; set; }

    public bool IsActive { get; set; }
}