using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.DTOs.SystemRolePermissions.SystemRolePermissionsRequest;
using RestaurantSaaS.Application.DTOs.SystemRolePermissions.SystemRolePermissionsResponse;
using RestaurantSaaS.Application.InterfacesService;

namespace RestaurantSaas_Api.Controllers
{
    [Authorize(Policy = "PlatformAdmin")]
    [Authorize]   
    [ApiController]
    [Route("api/SystemRolePermissions")]
    public class SystemRolePermissionsController : ControllerBase
    {
        private readonly ISystemRolePermissionService _service;
        public SystemRolePermissionsController(ISystemRolePermissionService service) => _service = service;

        [HttpPost("system-roles/{systemRoleId:int}", Name = "AssignSystemRolePermission")]
        public async Task<ActionResult<SystemRolePermissionResponse>> Assign(int systemRoleId, [FromBody] CreateSystemRolePermissionRequest request)
        {
            try
            {
                var result = await _service.AssignAsync(systemRoleId, request);
                return CreatedAtRoute("GetSystemRolePermissionsBySystemRole", new { systemRoleId }, result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpGet("system-roles/{systemRoleId:int}", Name = "GetSystemRolePermissionsBySystemRole")]
        public async Task<ActionResult<List<SystemRolePermissionResponse>>> GetAllBySystemRole(int systemRoleId)
            => Ok(await _service.GetAllBySystemRoleAsync(systemRoleId));

        [HttpDelete("{systemRolePermissionId:int}", Name = "RemoveSystemRolePermission")]
        public async Task<IActionResult> Remove(int systemRolePermissionId)
        {
            var removed = await _service.RemoveAsync(systemRolePermissionId);
            return removed ? NoContent() : NotFound(new { message = "Assignment not found." });
        }
    }
}
