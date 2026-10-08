using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.DTOs.Organizations.OrganizationsRequest;
using RestaurantSaaS.Application.DTOs.Platform.PlatformRequest;
using RestaurantSaaS.Application.InterfacesService;

namespace RestaurantSaas_Api.Controllers.Platform
{
 
    [Authorize(Policy = "PlatformAdmin")]
    [ApiController]
    [Route("api/platform/organizations")]
    public class PlatformOrganizationsController : ControllerBase
    {

        private readonly IPlatformOrganizationService _service;
        private readonly IAuthService _authService;

        public PlatformOrganizationsController(IPlatformOrganizationService service, IAuthService authService)
        {
            _service = service;
            _authService = authService;
        }
        [HttpGet] public async Task<ActionResult<List<OrganizationResponse>>> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{organizationId:int}")]
        public async Task<ActionResult<OrganizationResponse>> GetById(int organizationId)
        {
            var result = await _service.GetByIdAsync(organizationId);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPatch("{organizationId:int}/active")]
        public async Task<ActionResult<OrganizationResponse>> SetActive(int organizationId, [FromBody] SetActiveRequest request)
        {
            var result = await _service.SetActiveAsync(organizationId, request.IsActive);
            return result is null ? NotFound() : Ok(result);
        }
    }
}
