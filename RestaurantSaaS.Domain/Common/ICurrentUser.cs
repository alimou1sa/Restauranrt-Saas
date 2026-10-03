using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Domain.Common
{ 

    public interface ICurrentUser
    {
        int UserId { get; }
        string? Jti { get; }
        bool IsAuthenticated { get; }
        DateTime? ExpiresAtUtc { get; }
    }
}
