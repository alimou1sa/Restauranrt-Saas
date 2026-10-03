    using global::RestaurantSaaS.Application.DTOs.Auth.Request;
    using global::RestaurantSaaS.Application.DTOs.Auth.Response;
    using global::RestaurantSaaS.Application.InterfacesService;
    using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestaurantSaaS.Application.Common;
using RestaurantSaaS.Application.DTOs.RefreshTokens.RefreshTokensRequest;
using RestaurantSaaS.Domain.Common;
using RestaurantSaaS.Domain.Entities;
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
        private readonly ICurrentUser _currentUser;
        private readonly IRefreshTokenHasher _refreshTokenHasher;
        private readonly int _refreshTokenExpirationDays;

        private readonly IAccessTokenBlacklist _blacklist;
        public AuthService(IAppDbContext context, IPasswordHasher passwordHasher,IJwtTokenGenerator tokenGenerator, ICurrentUser currentUser, 
                IRefreshTokenSettings refreshTokenSettings,IRefreshTokenHasher refreshTokenHasher, IAccessTokenBlacklist blacklist)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
            _currentUser = currentUser;
            _refreshTokenExpirationDays = refreshTokenSettings.RefreshTokenExpirationDays;
            _refreshTokenHasher = refreshTokenHasher;
            _blacklist = blacklist;
        }




        public async Task<AuthTokenResponse> SelectOrganizationAsync(SelectOrganizationRequest request)
        {
            var organizationUser = await _context.OrganizationUsers
                .IgnoreQueryFilters().AsNoTracking()
                .Where(ou => ou.UserId == _currentUser.UserId && ou.OrganizationId == request.OrganizationId
                    && ou.IsActive && ou.RemovedAtUtc == null)
                .Select(ou => new
                {
                    ou.OrganizationUserId,
                    ou.OrganizationId,
                    ou.BranchId,
                    UserEmail = ou.User.Email,
                    UserIsActive = ou.User.IsActive,
                    OrganizationIsActive = ou.Organization.IsActive
                })
                .FirstOrDefaultAsync();

            if (organizationUser is null || !organizationUser.OrganizationIsActive || !organizationUser.UserIsActive)
                throw new UnauthorizedAccessException("You are not an active member of this organization.");

            var accessToken = BuildAccessToken(_currentUser.UserId, organizationUser.UserEmail,
                organizationUser.OrganizationId, organizationUser.OrganizationUserId, organizationUser.BranchId);

            var rawRefreshToken = await IssueRefreshTokenAsync(organizationUser.OrganizationUserId);

            return new AuthTokenResponse
            {
                Token = accessToken,
                RefreshToken = rawRefreshToken,
                OrganizationId = organizationUser.OrganizationId,
                OrganizationUserId = organizationUser.OrganizationUserId,
                BranchId = organizationUser.BranchId
            };
        }

        public async Task<AuthTokenResponse> RefreshAsync(RefreshTokenRequest request)
        {
            var tokenHash = _refreshTokenHasher.Hash(request.RefreshToken);

            var existing = await _context.RefreshTokens
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

            if (existing is null)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            if (existing.RevokedAtUtc is not null)
            {

                await RevokeAllForOrganizationUserAsync(existing.OrganizationUserId);
                throw new UnauthorizedAccessException("This refresh token has already been used. All sessions for this membership have been revoked.");
            }

            if (existing.ExpiresAtUtc <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Refresh token has expired.");

            var organizationUser = await _context.OrganizationUsers
                .IgnoreQueryFilters().AsNoTracking()
                .Where(ou => ou.OrganizationUserId == existing.OrganizationUserId && ou.IsActive && ou.RemovedAtUtc == null)
                .Select(ou => new
                {
                    ou.OrganizationUserId,
                    ou.OrganizationId,
                    ou.UserId,
                    ou.BranchId,
                    UserEmail = ou.User.Email,
                    UserIsActive = ou.User.IsActive,
                    OrganizationIsActive = ou.Organization.IsActive
                })
                .FirstOrDefaultAsync();

            if (organizationUser is null || !organizationUser.OrganizationIsActive || !organizationUser.UserIsActive)
                throw new UnauthorizedAccessException("This membership is no longer active.");

            await using var transaction = await _context.BeginTransactionAsync();
            try
            {
                var newRawToken = _refreshTokenHasher.GenerateRawToken();
                var newRefreshToken = new RefreshToken
                {
                    OrganizationUserId = organizationUser.OrganizationUserId,
                    TokenHash = _refreshTokenHasher.Hash(newRawToken),
                    ExpiresAtUtc = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
                    CreatedAtUtc = DateTime.UtcNow
                };
                _context.RefreshTokens.Add(newRefreshToken);
                await _context.SaveChangesAsync();

                existing.RevokedAtUtc = DateTime.UtcNow;
                existing.ReplacedByTokenId = newRefreshToken.RefreshTokenId;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var accessToken = BuildAccessToken(organizationUser.UserId, organizationUser.UserEmail,
                    organizationUser.OrganizationId, organizationUser.OrganizationUserId, organizationUser.BranchId);

                return new AuthTokenResponse
                {
                    Token = accessToken,
                    RefreshToken = newRawToken,
                    OrganizationId = organizationUser.OrganizationId,
                    OrganizationUserId = organizationUser.OrganizationUserId,
                    BranchId = organizationUser.BranchId
                };
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task LogoutAsync(LogoutRequest request)
        {
           
            if (_currentUser.Jti is not null && _currentUser.ExpiresAtUtc is not null)
            {
                var remaining = _currentUser.ExpiresAtUtc.Value - DateTime.UtcNow;
                _blacklist.Revoke(_currentUser.Jti, remaining);
            }

         
            var tokenHash = _refreshTokenHasher.Hash(request.RefreshToken);
            var existing = await _context.RefreshTokens
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

            if (existing is null || existing.RevokedAtUtc is not null)
                return;   

            var belongsToCurrentUser = await _context.OrganizationUsers
                .IgnoreQueryFilters()
                .AnyAsync(ou => ou.OrganizationUserId == existing.OrganizationUserId && ou.UserId == _currentUser.UserId);

            if (!belongsToCurrentUser)
                throw new UnauthorizedAccessException("This refresh token does not belong to your account.");

            existing.RevokedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }


        private string BuildAccessToken(int userId, string email, int organizationId, int organizationUserId, int? branchId)
        {
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(AppClaimTypes.OrganizationId, organizationId.ToString()),
            new(AppClaimTypes.OrganizationUserId, organizationUserId.ToString()),
            new(AppClaimTypes.TokenType, TokenTypes.Access),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
            if (branchId.HasValue)
                claims.Add(new Claim(AppClaimTypes.BranchId, branchId.Value.ToString()));

            return _tokenGenerator.GenerateToken(claims);
        }

        private async Task<string> IssueRefreshTokenAsync(int organizationUserId)
        {
            var rawToken = _refreshTokenHasher.GenerateRawToken();
            _context.RefreshTokens.Add(new RefreshToken
            {
                OrganizationUserId = organizationUserId,
                TokenHash = _refreshTokenHasher.Hash(rawToken),
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
                CreatedAtUtc = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return rawToken;
        }

        private async Task RevokeAllForOrganizationUserAsync(int organizationUserId)
        {
            await _context.RefreshTokens
                .IgnoreQueryFilters()
                .Where(rt => rt.OrganizationUserId == organizationUserId && rt.RevokedAtUtc == null)
                .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.RevokedAtUtc, DateTime.UtcNow));
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

        public async Task<List<OrganizationOptionResponse>> GetMyOrganizationsAsync()
        {
            return await GetActiveOrganizationsAsync(_currentUser.UserId);
        }


        private async Task<List<OrganizationOptionResponse>> GetActiveOrganizationsAsync(int userId)
        {

            return await _context.OrganizationUsers.IgnoreQueryFilters()
                .AsNoTracking()
                .Where(ou => ou.UserId == userId && ou.IsActive && ou.RemovedAtUtc == null && ou.Organization.IsActive)
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
