using global::RestaurantSaaS.Application.DTOs.Auth;
using global::RestaurantSaaS.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;
using RestaurantSaaS.Application.DTOs.Auth.Request;
using RestaurantSaaS.Application.DTOs.Auth.Response;
using RestaurantSaaS.Application.DTOs.RefreshTokens.RefreshTokensRequest;
using RestaurantSaaS.Application.InterfacesService;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        [Authorize(Policy = "PlatformAdmin")]
        [HttpPost("{organizationId:int}/impersonate")]
        public async Task<ActionResult<ImpersonationResponse>> Impersonate(int organizationId)
        {
            try { return Ok(await _authService.ImpersonateOrganizationAsync(organizationId)); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }


        [Authorize(Policy = "AuthenticatedAny")]   
        [HttpGet("me", Name = "GetMe")]
        public async Task<ActionResult<MeResponse>> GetMe()
        {
            try { return Ok(await _authService.GetMeAsync()); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
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


        [Authorize(Policy = "AuthenticatedAny")]
        [HttpGet( Name = "GetMyOrganizations")]
        public async Task<ActionResult<List<OrganizationOptionResponse>>> GetMyOrganizations()
        {
            var result = await _authService.GetMyOrganizationsAsync();
            return Ok(result);
        }

        [Authorize(Policy = "AuthenticatedAny")]
        [HttpPost("select-organization", Name = "SelectOrganization")]
        public async Task<ActionResult<AuthTokenResponse>> SelectOrganization([FromBody] SelectOrganizationRequest request)
        {
            try
            {
                var result = await _authService.SelectOrganizationAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }



        [AllowAnonymous]  
        [HttpPost("refresh", Name = "RefreshToken")]
        public async Task<ActionResult<AuthTokenResponse>> Refresh([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var result = await _authService.RefreshAsync(request);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
        }

     
      //  [AllowAnonymous]

        [Authorize]
        [HttpPost("logout", Name = "Logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            await _authService.LogoutAsync(request);
            return NoContent();
      
        }




    }
}


