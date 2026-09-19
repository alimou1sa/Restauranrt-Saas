using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.InterfacesService
{
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }
        int UserId { get; }

        int? OrganizationId { get; }

        int? OrganizationUserId { get; }

        int? BranchId { get; }
    }
}
