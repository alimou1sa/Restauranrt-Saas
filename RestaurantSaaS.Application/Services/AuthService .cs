    using global::RestaurantSaaS.Application.DTOs.Auth.Request;
    using global::RestaurantSaaS.Application.DTOs.Auth.Response;
    using global::RestaurantSaaS.Application.InterfacesService;
    using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestaurantSaaS.Application.Common;
using RestaurantSaaS.Infrastructure;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
namespace RestaurantSaaS.Application.Services
{

        public class AuthService : IAuthService
        {

            private static readonly TimeSpan PreAuthExpiration = TimeSpan.FromMinutes(5);

            private readonly IAppDbContext _context;
            private readonly IPasswordHasher _passwordHasher;
            private readonly IJwtTokenGenerator _tokenGenerator;

            public AuthService(IAppDbContext context,IPasswordHasher passwordHasher,IJwtTokenGenerator tokenGenerator)
            {
                _context = context;
                _passwordHasher = passwordHasher;
                _tokenGenerator = tokenGenerator;
            }

            public async Task<LoginResponse> LoginAsync(LoginRequest request)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                    throw new UnauthorizedAccessException("Invalid email or password.");

                if (!user.IsActive)
                    throw new UnauthorizedAccessException("This account is inactive.");

                var organizations = await GetActiveOrganizationsAsync(user.UserId);

                if (organizations.Count == 0)
                    throw new UnauthorizedAccessException("User is not assigned to any active organization.");

                user.LastLoginAtUtc = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var preAuthClaims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(AppClaimTypes.TokenType, TokenTypes.PreAuth),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

                var preAuthToken = _tokenGenerator.GenerateToken(preAuthClaims, PreAuthExpiration);

                return new LoginResponse
                {
                    Token = preAuthToken,
                    TokenType = TokenTypes.PreAuth,
                    Organizations = organizations
                };
            }

            public async Task<List<OrganizationOptionResponse>> GetMyOrganizationsAsync(int userId)
            {
                return await GetActiveOrganizationsAsync(userId);
            }

            public async Task<AuthTokenResponse> SelectOrganizationAsync(int userId, SelectOrganizationRequest request)
            {
                var organizationUser = await _context.OrganizationUsers.IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(ou => ou.Organization)
                    .Include(ou => ou.User)
                    .FirstOrDefaultAsync(ou =>
                        ou.UserId == userId &&
                        ou.OrganizationId == request.OrganizationId &&
                        ou.IsActive &&
                        ou.RemovedAtUtc == null);

                if (organizationUser is null
                    || !organizationUser.Organization.IsActive
                    || !organizationUser.User.IsActive)
                {
                    throw new UnauthorizedAccessException("You are not an active member of this organization.");
                }

                var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new(ClaimTypes.Email, organizationUser.User.Email),
                new(AppClaimTypes.OrganizationId, organizationUser.OrganizationId.ToString()),
                new(AppClaimTypes.OrganizationUserId, organizationUser.OrganizationUserId.ToString()),
                new(AppClaimTypes.TokenType, TokenTypes.Access),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

                if (organizationUser.BranchId.HasValue)
                    claims.Add(new Claim(AppClaimTypes.BranchId, organizationUser.BranchId.Value.ToString()));

                var accessToken = _tokenGenerator.GenerateToken(claims); 

                return new AuthTokenResponse
                {
                    Token = accessToken,
                    OrganizationId = organizationUser.OrganizationId,
                    OrganizationUserId = organizationUser.OrganizationUserId,
                    BranchId = organizationUser.BranchId
                };
            }

        private async Task<List<OrganizationOptionResponse>> GetActiveOrganizationsAsync(int userId)
        {

                return await _context.OrganizationUsers.IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(ou =>ou.UserId == userId &&ou.IsActive &&ou.RemovedAtUtc == null &&ou.Organization.IsActive)
                    .Select(ou => new OrganizationOptionResponse
                    {
                        OrganizationId = ou.OrganizationId,
                        OrganizationName = ou.Organization.Name,
                        BranchId = ou.BranchId,
                        BranchName = ou.Branch != null ? ou.Branch.Name : null
                    })
                    .ToListAsync();
            }
        }
    
}
