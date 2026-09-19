    using global::RestaurantSaaS.Application.DTOs.Roles.RoleRequest;
    using global::RestaurantSaaS.Application.DTOs.Roles.RoleResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Infrastructure;
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

        [HttpPost("organizations/{organizationId}/custom", Name = "CreateRoleByOrganization")]
        public async Task<ActionResult<RoleResponse>> CreateCustom(int organizationId,CreateCustomRoleRequest request)
        {
            try
            {
                if (organizationId <= 0)
                    return BadRequest(new { message = "Organization ID must be greater than 0." });


                var role = await _roleService.CreateCustomAsync(organizationId, request);

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

        [HttpPost("organizations/{organizationId}/system/{systemRoleId}", Name = "AddSystemRole")]
        public async Task<ActionResult<RoleResponse>> AddSystemRole(int organizationId,int systemRoleId)
        {
            try
            {
                if (organizationId <= 0)
                    return BadRequest(new { message = "Organization ID must be greater than 0." });
                if (systemRoleId <= 0)
                    return BadRequest(new { message = "System Role ID must be greater than 0." });
                var role = await _roleService
                    .AddSystemRoleAsync(organizationId, systemRoleId);

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

        [HttpGet("{roleId}", Name = "GetRoleByOrganiGetByIdzation")]
        public async Task<ActionResult<RoleResponse>> GetById(int roleId)
        {
            var role = await _roleService
                .GetByIdAsync(roleId);

            if (role is null)
                return NotFound();

            return Ok(role);
        }

        [HttpGet("organizations/{organizationId}", Name = "GetByOrganization")]
        public async Task<ActionResult<List<RoleResponse>>> GetAllByOrganization(int organizationId)
        {
            var roles = await _roleService.GetAllByOrganizationAsync(organizationId);

            return Ok(roles);
        }

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
