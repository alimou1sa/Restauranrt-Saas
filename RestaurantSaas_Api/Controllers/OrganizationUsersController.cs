    using global::RestaurantSaaS.Application.DTOs.OrganizationUsers.OrganizationUserRequest;
    using global::RestaurantSaaS.Application.DTOs.OrganizationUsers.OrganizationUserResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace RestaurantSaas_Api.Controllers
{

    //[Authorize]
    [ApiController]
    [Route("api/OrganizationUsers")]
    public class OrganizationUsersController : ControllerBase
    {
        private readonly IOrganizationUserService _organizationUserService;

        public OrganizationUsersController(IOrganizationUserService organizationUserService)
        {
            _organizationUserService = organizationUserService;
        }

        [HttpPost("organizations/{organizationId:int}", Name = "CreateOrganizationUser")]
        public async Task<ActionResult<OrganizationUserResponse>> Create(int organizationId,[FromBody] CreateOrganizationUserRequest request)
        {
            if (organizationId <= 0)return BadRequest(new { message = "Organization ID must be greater than 0." });

            try
            {
                var organizationUser = await _organizationUserService.CreateAsync(organizationId, request);
                return CreatedAtRoute("GetOrganizationUserById", new { organizationUserId = organizationUser.OrganizationUserId }, organizationUser);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("{organizationUserId:int}", Name = "GetOrganizationUserById")]
        public async Task<ActionResult<OrganizationUserResponse>> GetById(int organizationUserId)
        {
            if (organizationUserId <= 0)return BadRequest(new { message = "Organization member ID must be greater than 0." });

            var organizationUser = await _organizationUserService.GetByIdAsync(organizationUserId);

            return organizationUser is null? NotFound(new { message = "Organization member not found." }): Ok(organizationUser);
        }

        [HttpGet("organizations/{organizationId:int}", Name = "GetOrganizationUsersByOrganization")]
        public async Task<ActionResult<IEnumerable<OrganizationUserResponse>>> GetAllByOrganization(int organizationId)
        {
            if (organizationId <= 0)
                return BadRequest(new { message = "Organization ID must be greater than 0." });

            var organizationUsers = await _organizationUserService.GetAllByOrganizationAsync(organizationId);
            return Ok(organizationUsers);
        }

        [HttpPut("{organizationUserId:int}", Name = "UpdateOrganizationUser")]
        public async Task<ActionResult<OrganizationUserResponse>> Update(int organizationUserId,[FromBody] UpdateOrganizationUserRequest request)
        {
            if (organizationUserId <= 0)
                return BadRequest(new { message = "Organization member ID must be greater than 0." });

            try
            {
                var organizationUser = await _organizationUserService.UpdateAsync(organizationUserId, request);

                return organizationUser is null
                    ? NotFound(new { message = "Organization member not found." })
                    : Ok(organizationUser);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }


        [HttpDelete("{organizationUserId:int}", Name = "DeleteOrganizationUser")]
        public async Task<IActionResult> Delete(int organizationUserId)
        {
            if (organizationUserId <= 0)
                return BadRequest(new { message = "Organization member ID must be greater than 0." });

            var deleted = await _organizationUserService.DeleteAsync(organizationUserId);

            return deleted
                ? NoContent()
                : NotFound(new { message = "Organization member not found." });
        }
    }
}
