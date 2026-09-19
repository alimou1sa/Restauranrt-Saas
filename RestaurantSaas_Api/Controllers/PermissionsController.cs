    using global::RestaurantSaaS.Application.DTOs.Permissions.PermissionRequest;
    using global::RestaurantSaaS.Application.DTOs.Permissions.PermissionResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
namespace RestaurantSaas_Api.Controllers
{
    [Authorize]

    [ApiController]
    [Route("api/permissions")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpPost(Name = "CreatePermission")]
        public async Task<ActionResult<PermissionResponse>> Create([FromBody] CreatePermissionRequest request)
        {
            try
            {
                var permission = await _permissionService.CreateAsync(request);
                return CreatedAtRoute("GetPermissionById", new { permissionId = permission.PermissionId }, permission);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("{permissionId:int}", Name = "GetPermissionById")]
        public async Task<ActionResult<PermissionResponse>> GetById(int permissionId)
        {
            if (permissionId <= 0)
                return BadRequest(new { message = "Permission ID must be greater than 0." });

            var permission = await _permissionService.GetByIdAsync(permissionId);

            return permission is null
                ? NotFound(new { message = "Permission not found." })
                : Ok(permission);
        }

        [HttpGet(Name = "GetAllPermissions")]
        public async Task<ActionResult<IEnumerable<PermissionResponse>>> GetAll()
        {
            var permissions = await _permissionService.GetAllAsync();
            return Ok(permissions);
        }

        [HttpPut("{permissionId:int}", Name = "UpdatePermission")]
        public async Task<ActionResult<PermissionResponse>> Update(int permissionId,[FromBody] UpdatePermissionRequest request)
        {
            if (permissionId <= 0)
                return BadRequest(new { message = "Permission ID must be greater than 0." });

            var permission = await _permissionService.UpdateAsync(permissionId, request);

            return permission is null
                ? NotFound(new { message = "Permission not found." })
                : Ok(permission);
        }

        [HttpDelete("{permissionId:int}", Name = "DeletePermission")]
        public async Task<IActionResult> Delete(int permissionId)
        {
            if (permissionId <= 0)
                return BadRequest(new { message = "Permission ID must be greater than 0." });

            try
            {
                var deleted = await _permissionService.DeleteAsync(permissionId);

                return deleted
                    ? NoContent()
                    : NotFound(new { message = "Permission not found." });
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "This permission cannot be deleted because related data still exists." });
            }
        }
    }
}
