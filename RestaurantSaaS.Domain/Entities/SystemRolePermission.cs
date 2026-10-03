using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Domain.Entities
{
    public partial class SystemRolePermission
    {
        public int SystemRolePermissionId { get; set; }

        public int SystemRoleId { get; set; }

        public int PermissionId { get; set; }

        public virtual SystemRole SystemRole { get; set; } = null!;

        public virtual Permission Permission { get; set; } = null!;
    }
}
