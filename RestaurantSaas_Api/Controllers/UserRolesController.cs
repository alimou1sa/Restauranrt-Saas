    using global::RestaurantSaaS.Application.DTOs.UserRoles.RoleRequest;
    using global::RestaurantSaaS.Application.DTOs.UserRoles.RoleResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace RestaurantSaas_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/UserRoles")]
    public class UserRolesController : ControllerBase
    {
        private readonly IUserRoleService _userRoleService;

        public UserRolesController(IUserRoleService userRoleService)
        {
            _userRoleService = userRoleService;
        }

        [HttpPost("{organizationUserId:int}", Name = "AssignUserRole")]
        public async Task<ActionResult<UserRoleResponse>> Assign(int organizationUserId,[FromBody] AssignRoleRequest request)
        {
            if (organizationUserId <= 0)
                return BadRequest(new { message = "Organization member ID must be greater than 0." });

            try
            {
                var userRole = await _userRoleService.AssignAsync(organizationUserId, request);
                return CreatedAtRoute("GetUserRolesByOrganizationUser",new { organizationUserId },userRole);
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

        [HttpGet("{organizationUserId:int}", Name = "GetUserRolesByOrganizationUser")]
        public async Task<ActionResult<IEnumerable<UserRoleResponse>>> GetAllByOrganizationUser(int organizationUserId)
        {
            if (organizationUserId <= 0)
                return BadRequest(new { message = "Organization member ID must be greater than 0." });

            var userRoles = await _userRoleService.GetAllByOrganizationUserAsync(organizationUserId);
            return Ok(userRoles);
        }

        [HttpDelete("user-roles/{userRoleId:int}", Name = "RemoveUserRole")]
        public async Task<IActionResult> Remove(int userRoleId)
        {
            if (userRoleId <= 0)
                return BadRequest(new { message = "User role ID must be greater than 0." });

            var removed = await _userRoleService.RemoveAsync(userRoleId);

            return removed
                ? NoContent()
                : NotFound(new { message = "Role assignment not found." });
        }
    }
}
