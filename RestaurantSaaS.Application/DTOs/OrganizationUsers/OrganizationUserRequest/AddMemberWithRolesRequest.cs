using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.OrganizationUsers.OrganizationUserRequest
{
    public class AddMemberWithRolesRequest
    {
        [Required]
        public int UserId { get; set; }

        public int? BranchId { get; set; }

        [Required, MinLength(1)]
        public List<int> RoleIds { get; set; } = new();
    }
}
