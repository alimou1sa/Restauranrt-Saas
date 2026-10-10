using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.Auth.Response
{
    public class MeResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string? LastName { get; set; }



        public bool IsPlatformAdmin { get; set; }  

        public int? OrganizationId { get; set; }         
        public string? OrganizationName { get; set; }    
        public int? OrganizationUserId { get; set; }


        public int? BranchId { get; set; }
        public string? BranchName { get; set; }

        public List<string> Roles { get; set; } = new();

        public List<string> Permissions { get; set; } = new();
    }
}
