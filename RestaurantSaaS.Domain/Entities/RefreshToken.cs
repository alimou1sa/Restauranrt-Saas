using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Domain.Entities
{

    public partial class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public int OrganizationUserId { get; set; }
        public string TokenHash { get; set; } = null!;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public int? ReplacedByTokenId { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public virtual OrganizationUser OrganizationUser { get; set; } = null!;
    }
}
