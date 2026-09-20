using global::RestaurantSaaS.Application.DTOs.Auth;
using global::RestaurantSaaS.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;
using RestaurantSaaS.Application.DTOs.Auth.Request;
using RestaurantSaaS.Application.DTOs.Auth.Response;
using RestaurantSaaS.Application.InterfacesService;
using System.Security.Claims;

using System.IdentityModel.Tokens.Jwt;
namespace RestaurantSaas_Api.Controllers
{

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login", Name = "Login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }


        [Authorize]
        [HttpGet("organizations", Name = "GetMyOrganizations")]
        public async Task<ActionResult<List<OrganizationOptionResponse>>> GetMyOrganizations()
        {
            var result = await _authService.GetMyOrganizationsAsync(GetUserId());
            return Ok(result);
        }

        [Authorize]
        [HttpPost("select-organization", Name = "SelectOrganization")]
        public async Task<ActionResult<AuthTokenResponse>> SelectOrganization([FromBody] SelectOrganizationRequest request)
        {
            try
            {
                var result = await _authService.SelectOrganizationAsync(GetUserId(), request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        private int GetUserId()
        {
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (sub is null || !int.TryParse(sub, out var userId))
                throw new UnauthorizedAccessException("Invalid token.");
            return userId;
        }
    }
}


