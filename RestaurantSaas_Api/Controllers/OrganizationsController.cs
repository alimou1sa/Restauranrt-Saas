using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs.Organizations.OrganizationsRequest;
using RestaurantSaaS.Application.InterfacesService;

namespace RestaurantSaaS.API.Controllers
{
  //  [Authorize]
    [ApiController]
    [Route("api/organizations")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationsController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpPost(Name = "CreateOrganization")]
        public async Task<ActionResult<OrganizationResponse>> Create([FromBody] CreateOrganizationRequest request)
        {
            try
            {
                var organization = await _organizationService.CreateAsync(request);
                return CreatedAtAction("GetOrganizationById", new { organizationId = organization.OrganizationId }, organization);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("{organizationId:int}", Name = "GetOrganizationById")]
        public async Task<ActionResult<OrganizationResponse>> GetById(int organizationId)
        {
            if(organizationId<1) return BadRequest($"Not accepted ID {organizationId}");

            var organization = await _organizationService.GetByIdAsync(organizationId);

            return organization is null ? NotFound() : Ok(organization);
        }

        [HttpGet(Name = "GetOrganizations")]
        public async Task<ActionResult<IEnumerable<OrganizationResponse>>> GetAll()
        {
            var organizations = await _organizationService.GetAllAsync();
            return Ok(organizations);
        }

        [HttpPut("{organizationId:int}", Name = "UpdateOrganization")]
        public async Task<ActionResult<OrganizationResponse>> Update(int organizationId,[FromBody] UpdateOrganizationRequest request)
        {
            if(organizationId<1) return BadRequest($"Not accepted ID {organizationId}");


            var organization = await _organizationService.UpdateAsync(organizationId, request);
            return organization is null ? NotFound() : Ok(organization);
        }

        [HttpDelete("{organizationId:int}", Name = "DeleteOrganization")]
        public async Task<IActionResult> Delete(int organizationId)
        {
            try
            {
                var deleted = await _organizationService.DeleteAsync(organizationId);
                return deleted ? NoContent() : NotFound();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "This organization cannot be deleted because related data still exists." });
            }
        }
    }
}
