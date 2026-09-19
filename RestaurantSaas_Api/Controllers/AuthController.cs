using global::RestaurantSaaS.Application.DTOs.Auth;
using global::RestaurantSaaS.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;
using RestaurantSaaS.Application.DTOs.Auth.Response;
using RestaurantSaaS.Application.DTOs.Auth.Request;
using RestaurantSaaS.Application.InterfacesService;
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
    }
}


