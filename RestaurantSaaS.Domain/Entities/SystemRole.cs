using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Domain.Entities
{
    public partial class SystemRole
    {
        public int SystemRoleId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }


        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    }
}
