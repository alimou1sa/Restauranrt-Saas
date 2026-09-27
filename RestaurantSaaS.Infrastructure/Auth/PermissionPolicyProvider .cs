using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Options;
namespace RestaurantSaaS.Infrastructure.Auth
{

        public class PermissionPolicyProvider : IAuthorizationPolicyProvider
        {
            private const string Prefix = "Permission:";
            private readonly DefaultAuthorizationPolicyProvider _fallback;

            public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
            {
                _fallback = new DefaultAuthorizationPolicyProvider(options);
            }

            public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();
            public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

            public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
            {
                if (policyName.StartsWith(Prefix, StringComparison.Ordinal))
                {
                    var code = policyName[Prefix.Length..];
                    var policy = new AuthorizationPolicyBuilder()
                        .AddRequirements(new PermissionRequirement(code)).Build();
                    return Task.FromResult<AuthorizationPolicy?>(policy);
                }

                return _fallback.GetPolicyAsync(policyName);
            }
        }
    
}
