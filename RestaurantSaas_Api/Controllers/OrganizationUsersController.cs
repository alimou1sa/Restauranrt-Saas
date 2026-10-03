    using global::RestaurantSaaS.Application.DTOs.OrganizationUsers.OrganizationUserRequest;
    using global::RestaurantSaaS.Application.DTOs.OrganizationUsers.OrganizationUserResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Infrastructure.Auth;
namespace RestaurantSaas_Api.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/OrganizationUsers")]
    public class OrganizationUsersController : ControllerBase
    {
        private readonly IOrganizationUserService _organizationUserService;

        public OrganizationUsersController(IOrganizationUserService organizationUserService)
        {
            _organizationUserService = organizationUserService;
        }


        [HasPermission("user.manage")]
        [HttpPost("with-roles", Name = "AddOrganizationUserWithRoles")]
        public async Task<ActionResult<OrganizationUserResponse>> AddWithRoles([FromBody] AddMemberWithRolesRequest request)
        {
            try
            {
                var result = await _organizationUserService.AddMemberWithRolesAsync(request);
                return CreatedAtRoute("GetOrganizationUserById", new { organizationUserId = result.OrganizationUserId }, result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }



        [HasPermission("user.manage")]
        [HttpPost( Name = "CreateOrganizationUser")]
        public async Task<ActionResult<OrganizationUserResponse>> Create([FromBody] CreateOrganizationUserRequest request)
        {

            try
            {
                var organizationUser = await _organizationUserService.CreateAsync( request);
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

        [HasPermission("user.read")]
        [HttpGet("{organizationUserId:int}", Name = "GetOrganizationUserById")]
        public async Task<ActionResult<OrganizationUserResponse>> GetById(int organizationUserId)
        {
            if (organizationUserId <= 0)return BadRequest(new { message = "Organization member ID must be greater than 0." });

            var organizationUser = await _organizationUserService.GetByIdAsync(organizationUserId);

            return organizationUser is null? NotFound(new { message = "Organization member not found." }): Ok(organizationUser);
        }

        [HasPermission("user.read")]
        [HttpGet( Name = "GetOrganizationUsers")]
        public async Task<ActionResult<IEnumerable<OrganizationUserResponse>>> GetOrganizationUsers()
        {


            var organizationUsers = await _organizationUserService.GetAllAsync();
            return Ok(organizationUsers);
        }

        [HasPermission("user.manage")]
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

        [HasPermission("user.manage")]
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
