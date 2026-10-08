using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.Common
{

    public static class AppClaimTypes
    {
        public const string OrganizationId = "org_id";
        public const string OrganizationUserId = "org_user_id";
        public const string BranchId = "branch_id";
        public const string TokenType = "token_type";

        public const string IsPlatformAdmin = "is_platform_admin";
    }

    public static class TokenTypes
    {
        public const string PreAuth = "org_selection";
        public const string Access = "access";
        public const string PlatformAdmin = "platform_admin";
    }
    
}
