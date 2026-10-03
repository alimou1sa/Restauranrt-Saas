    using global::RestaurantSaaS.Application.DTOs.Roles.RoleRequest;
    using global::RestaurantSaaS.Application.DTOs.Roles.RoleResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Infrastructure;
using RestaurantSaaS.Infrastructure.Auth;
namespace RestaurantSaas_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HasPermission("role.manage")]
        [HttpPost("custom", Name = "CreateRole")]
        public async Task<ActionResult<RoleResponse>> CreateRole(CreateCustomRoleRequest request)
        {
            try
            {


                var role = await _roleService.CreateCustomAsync( request);

                return CreatedAtRoute("GetRoleByroleId", new { roleId = role.RoleId },role);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HasPermission("role.manage")]
        [HttpPost("system/{systemRoleId}", Name = "AddSystemRole")]
        public async Task<ActionResult<RoleResponse>> AddSystemRole(int systemRoleId)
        {
            try
            {
       
                if (systemRoleId <= 0)
                    return BadRequest(new { message = "System Role ID must be greater than 0." });
                var role = await _roleService.AddSystemRoleAsync(systemRoleId);

                return CreatedAtRoute("GetRoleByOrganiGetByIdzation", new { roleId = role.RoleId },role);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HasPermission("role.read")]
        [HttpGet("Role/{roleId}", Name = "GetRoleByroleId")]
        public async Task<ActionResult<RoleResponse>> GetById(int roleId)
        {
            var role = await _roleService
                .GetByIdAsync(roleId);

            if (role is null)
                return NotFound();

            return Ok(role);
        }

        [HasPermission("role.read")]
        [HttpGet( Name = "GetRoles")]
        public async Task<ActionResult<List<RoleResponse>>> GetRoles()
        {
            var roles = await _roleService.GetAllAsync();

            return Ok(roles);
        }

        [HasPermission("role.manage")]
        [HttpPut("{roleId}",Name ="UpdateRole")]
        public async Task<ActionResult<RoleResponse>> UpdateCustom(int roleId,UpdateCustomRoleRequest request)
        {
            try
            {
                if (roleId <= 0)
                    return BadRequest(new { message = "role ID must be greater than 0." });
                var role = await _roleService
                    .UpdateCustomAsync(roleId, request);

                if (role is null)
                    return NotFound();

                return Ok(role);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HasPermission("role.manage")]
        [HttpDelete("{roleId}",Name = "DeleteRole")]
        public async Task<IActionResult> DeleteCustom(int roleId)
        {
            try
            {
                var deleted = await _roleService
                    .DeleteCustomAsync(roleId);

                if (!deleted)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
