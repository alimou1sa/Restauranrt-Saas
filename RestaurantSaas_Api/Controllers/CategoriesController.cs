    using global::RestaurantSaaS.Application.DTOs.Categories.CategoriesRequest;
    using global::RestaurantSaaS.Application.DTOs.Categories.CategoriesResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Infrastructure.Auth;
namespace RestaurantSaas_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HasPermission("menu.manage")]
        [HttpPost("menus/{menuId:int}",Name = "CreateCategory")]
        public async Task<ActionResult<CategoryResponse>> Create(int menuId,[FromBody] CreateCategoryRequest request)
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
                var category = await _categoryService.CreateAsync(menuId,request);

                return CreatedAtRoute("GetCategoryById",new { categoryId = category.CategoryId },category);
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

        [HasPermission("menu.read")]
        [HttpGet("{categoryId:int}",Name = "GetCategoryById")]
        public async Task<ActionResult<CategoryResponse>> GetById(int categoryId)
        {
            if (categoryId <= 0)
            {
                return BadRequest(new
                {
                    message = "Category ID must be greater than 0."
                });
            }

            var category = await _categoryService.GetByIdAsync(categoryId);

            return category is null
                ? NotFound(new
                {
                    message = "Category not found."
                })
                : Ok(category);
        }

        [HasPermission("menu.read")]
        [HttpGet("menus/{menuId:int}",Name = "GetCategoriesByMenu")]
        public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAllByMenu(int menuId)
        {
            if (menuId <= 0)
            {
                return BadRequest(new
                {
                    message = "Menu ID must be greater than 0."
                });
            }

            var categories = await _categoryService.GetAllByMenuAsync(menuId);

            return Ok(categories);
        }

        [HasPermission("menu.manage")]
        [HttpPut("{categoryId:int}",Name = "UpdateCategory")]
        public async Task<ActionResult<CategoryResponse>> Update(int categoryId,[FromBody] UpdateCategoryRequest request)
        {
            if (categoryId <= 0)
            {
                return BadRequest(new
                {
                    message = "Category ID must be greater than 0."
                });
            }

            try
            {
                var category = await _categoryService.UpdateAsync(categoryId,request);

                return category is null
                    ? NotFound(new
                    {
                        message = "Category not found."
                    })
                    : Ok(category);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [HasPermission("menu.manage")]
        [HttpDelete("{categoryId:int}",Name = "DeleteCategory")]
        public async Task<IActionResult> Delete(int categoryId)
        {
            if (categoryId <= 0)
            {
                return BadRequest(new
                {
                    message = "Category ID must be greater than 0."
                });
            }

            try
            {
                var deleted = await _categoryService.DeleteAsync(categoryId);

                return deleted
                    ? NoContent()
                    : NotFound(new
                    {
                        message = "Category not found."
                    });
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message =
                        "This category cannot be deleted because related data still exists."
                });
            }
        }
    }
}
