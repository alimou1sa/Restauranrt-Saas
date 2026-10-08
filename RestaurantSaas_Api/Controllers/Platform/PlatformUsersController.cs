using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.DTOs.Platform.PlatformRequest;
using RestaurantSaaS.Application.DTOs.Users.UserResponse;
using RestaurantSaaS.Application.InterfacesService;

namespace RestaurantSaas_Api.Controllers.Platform
{
    // Controllers/Platform/PlatformUsersController.cs
    [Authorize(Policy = "PlatformAdmin")]
    [ApiController]
    [Route("api/platform/users")]
    public class PlatformUsersController : ControllerBase
    {
        private readonly IPlatformUserService _service;
        public PlatformUsersController(IPlatformUserService service) => _service = service;

        [HttpGet] public async Task<ActionResult<List<UserResponse>>> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{userId:int}")]
        public async Task<ActionResult<UserResponse>> GetById(int userId)
        {
            var result = await _service.GetByIdAsync(userId);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPatch("{userId:int}/active")]
        public async Task<ActionResult<UserResponse>> SetActive(int userId, [FromBody] SetActiveRequest request)
        {
            var result = await _service.SetActiveAsync(userId, request.IsActive);
            return result is null ? NotFound() : Ok(result);
        }
    }
}
