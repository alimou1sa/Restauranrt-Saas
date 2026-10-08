    using global::RestaurantSaaS.Application.InterfacesService;
    using Microsoft.AspNetCore.Authorization;
using RestaurantSaaS.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RestaurantSaaS.Infrastructure.Auth
{

        public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
        {
            private readonly ICurrentUser _currentUser;
        private readonly ICurrentTenant _currentTenant;
        private readonly IPermissionService _permissionService;

            public PermissionAuthorizationHandler(ICurrentUser currentUser, ICurrentTenant currentTenant, IPermissionService permissionService)
            {
                _currentUser = currentUser;
                _currentTenant = currentTenant;
                _permissionService = permissionService;
            }
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {

            if (_currentUser.IsPlatformAdmin && _currentTenant.OrganizationId.HasValue)
            {
                context.Succeed(requirement);
                return;
            }

            if (!_currentTenant.OrganizationUserId.HasValue)
                return;

            var permissions = await _permissionService.GetPermissionsAsync(_currentTenant.OrganizationUserId.Value);

            if (permissions.Contains(requirement.PermissionCode))
                context.Succeed(requirement);
        }
    

        }
    
}
