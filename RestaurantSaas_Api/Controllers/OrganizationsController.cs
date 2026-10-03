using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs.Organizations.OrganizationsRequest;
using RestaurantSaaS.Application.InterfacesService;
using RestaurantSaaS.Infrastructure.Auth;

namespace RestaurantSaaS.API.Controllers
{

    [ApiController]
    [Route("api/organizations")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationsController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }
  

        [HttpPost("current",Name = "CreateOrganization")]
        public async Task<ActionResult<OrganizationResponse>> Create([FromBody] CreateOrganizationRequest request)
        {
            try
            {
                var organization = await _organizationService.CreateAsync(request);
                return CreatedAtRoute("GetCurrentOrganization", null, organization);

       
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HasPermission("organization.read")]
        [HttpGet("current", Name = "GetCurrentOrganization")]
        public async Task<ActionResult<OrganizationResponse>> GetCurrentOrganization()
        {
  

            var organization = await _organizationService.GetAsync();

            return organization is null ? NotFound() : Ok(organization);
        }

        [HttpGet(Name = "GetOrganizations")]
        public async Task<ActionResult<IEnumerable<OrganizationResponse>>> GetAll()
        {
            var organizations = await _organizationService.GetAllAsync();
            return Ok(organizations);
        }

        [HasPermission("organization.update")]
        [HttpPut("current",Name = "UpdateOrganization")]
        public async Task<ActionResult<OrganizationResponse>> Update([FromBody] UpdateOrganizationRequest request)
        {
    
            var organization = await _organizationService.UpdateAsync( request);
            return organization is null ? NotFound() : Ok(organization);
        }

        [HttpDelete("current", Name = "DeleteOrganization")]
        public async Task<IActionResult> Delete()
        {
            try
            {
                var deleted = await _organizationService.DeleteAsync();
                return deleted ? NoContent() : NotFound();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "This organization cannot be deleted because related data still exists." });
            }
        }
    }
}
