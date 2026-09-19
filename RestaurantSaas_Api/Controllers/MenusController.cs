using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Application.DTOs.Menus.MenusRequest;
using RestaurantSaaS.Application.DTOs.Menus.MenusRespose;
using RestaurantSaaS.Application.InterfacesService;

namespace RestaurantSaas_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Menus")]
    public class MenusController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenusController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpPost("branches/{branchId:int}",Name = "CreateMenu")]
        public async Task<ActionResult<MenuResponse>> Create(int branchId,[FromBody] CreateMenuRequest request)
        {
            if (branchId <= 0)
            {
                return BadRequest(new
                {
                    message = "Branch ID must be greater than 0."
                });
            }

            try
            {
                var menu = await _menuService.CreateAsync(branchId,request);

                return CreatedAtRoute("GetMenuById",new { menuId = menu.MenuId },menu);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("{menuId:int}",Name = "GetMenuById")]
        public async Task<ActionResult<MenuResponse>> GetById(int menuId)
        {
            if (menuId <= 0)
            {
                return BadRequest(new
                {
                    message = "Menu ID must be greater than 0."
                });
            }

            var menu = await _menuService.GetByIdAsync(menuId);

            return menu is null
                ? NotFound(new
                {
                    message = "Menu not found."
                })
                : Ok(menu);
        }

        [HttpGet("branches/{branchId:int}",Name = "GetMenusByBranch")]
        public async Task<ActionResult<IEnumerable<MenuResponse>>> GetAllByBranch(int branchId)
        {
            if (branchId <= 0)
            {
                return BadRequest(new
                {
                    message = "Branch ID must be greater than 0."
                });
            }

            var menus = await _menuService.GetAllByBranchAsync(branchId);

            return Ok(menus);
        }

        [HttpPut("{menuId:int}",Name = "UpdateMenu")]
        public async Task<ActionResult<MenuResponse>> Update(int menuId,[FromBody] UpdateMenuRequest request)
        {
            if (menuId <= 0)
            {
                return BadRequest(new
                {
                    message = "Menu ID must be greater than 0."
                });
            }

            try
            {
                var menu = await _menuService.UpdateAsync(menuId,request);

                return menu is null
                    ? NotFound(new
                    {
                        message = "Menu not found."
                    })
                    : Ok(menu);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpDelete("{menuId:int}",Name = "DeleteMenu")]
        public async Task<IActionResult> Delete(int menuId)
        {
            if (menuId <= 0)
            {
                return BadRequest(new
                {
                    message = "Menu ID must be greater than 0."
                });
            }

            try
            {
                var deleted = await _menuService.DeleteAsync(menuId);

                return deleted
                    ? NoContent()
                    : NotFound(new
                    {
                        message = "Menu not found."
                    });
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "This menu cannot be deleted because related data still exists."
                });
            }
        }
    }
}
