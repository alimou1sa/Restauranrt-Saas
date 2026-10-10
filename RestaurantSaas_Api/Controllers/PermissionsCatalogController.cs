using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.DTOs.Permissions.PermissionResponse;
using RestaurantSaaS.Application.InterfacesService;
using RestaurantSaaS.Infrastructure.Auth;

namespace RestaurantSaas_Api.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/permissions/catalog")]
    public sealed class PermissionsCatalogController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionsCatalogController(
            IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet(Name = "GetPermissionCatalog")]
        [HasPermission("role.manage")]
        public async Task<ActionResult<IReadOnlyList<PermissionResponse>>>
            GetCatalog()
        {
            var permissions = await _permissionService.GetAllAsync();

            return Ok(
                permissions
                    .OrderBy(p => p.Code)
                    .ToList());
        }
    }

}
