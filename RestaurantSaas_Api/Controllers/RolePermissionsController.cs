    using global::RestaurantSaaS.Application.DTOs.RolePermissions.RolePermissionsRequest;
    using global::RestaurantSaaS.Application.DTOs.RolePermissions.RolePermissionsResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace RestaurantSaas_Api.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/RolePermissions")]
    public class RolePermissionsController : ControllerBase
    {
        private readonly IRolePermissionService _rolePermissionService;

        public RolePermissionsController(IRolePermissionService rolePermissionService)
        {
            _rolePermissionService = rolePermissionService;
        }

        [HttpPost("roles/{roleId:int}/", Name = "AssignRolePermission")]
        public async Task<ActionResult<RolePermissionResponse>> Assign(int roleId,[FromBody] AssignPermissionRequest request)
        {
            if (roleId <= 0)
                return BadRequest(new { message = "Role ID must be greater than 0." });

            try
            {
                var rolePermission = await _rolePermissionService.AssignAsync(roleId, request);
                return CreatedAtRoute("GetRolePermissionsByRole",new { roleId },rolePermission);
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

        [HttpGet("roles/{roleId:int}/", Name = "GetRolePermissionsByRole")]
        public async Task<ActionResult<IEnumerable<RolePermissionResponse>>> GetAllByRole(int roleId)
        {
            if (roleId <= 0)
                return BadRequest(new { message = "Role ID must be greater than 0." });

            var rolePermissions = await _rolePermissionService.GetAllByRoleAsync(roleId);
            return Ok(rolePermissions);
        }

        [HttpDelete("{rolePermissionId:int}", Name = "RemoveRolePermission")]
        public async Task<IActionResult> Remove(int rolePermissionId)
        {
            if (rolePermissionId <= 0)
                return BadRequest(new { message = "Role permission ID must be greater than 0." });

            var removed = await _rolePermissionService.RemoveAsync(rolePermissionId);

            return removed
                ? NoContent()
                : NotFound(new { message = "Permission assignment not found." });
        }
    }
}
