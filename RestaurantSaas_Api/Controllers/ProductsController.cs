    using global::RestaurantSaaS.Application.DTOs.Products.ProductsRequest;
    using global::RestaurantSaaS.Application.DTOs.Products.ProductsResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Infrastructure;
namespace RestaurantSaas_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost("categories/{categoryId:int}", Name = "CreateProduct")]
        public async Task<ActionResult<ProductDetailsResponse>> Create(int categoryId, [FromBody] CreateProductRequest request)
        {
            if (categoryId < 1) return BadRequest($"Not accepted ID {categoryId}");
            try
            {
                var product = await _productService.CreateAsync(categoryId, request);
                return CreatedAtRoute("GetProductByID", new { productId = product.ProductId }, product);
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

        [HttpGet("{productId:int}", Name = "GetProductByID")]
        public async Task<ActionResult<ProductDetailsResponse>> GetById(int productId)
        {
            if (productId < 1) return BadRequest($"Not accepted ID {productId}");
            var product = await _productService.GetByIdAsync(productId);
            return product is null ? NotFound() : Ok(product);
        }


        [HttpGet("branches/{branchId:int}", Name = "GetAllProduct")]
        public async Task<ActionResult<IEnumerable<ProductListResponse>>> GetAllByBranch(int branchId)
        {
            var products = await _productService.GetAllByBranchAsync(branchId);
            return Ok(products);
        }

        [HttpPut("{productId:int}", Name = "UpdateProduct")]
        public async Task<ActionResult<ProductDetailsResponse>> Update(int productId, [FromBody] UpdateProductRequest request)
        {
            if (productId < 1) return BadRequest($"Not accepted ID {productId}");
            try
            {
                var product = await _productService.UpdateAsync(productId, request);
                return product is null ? NotFound() : Ok(product);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPatch("{productId:int}/availability", Name = "UpdateAvailabilityProduct")]
        public async Task<ActionResult<ProductDetailsResponse>> UpdateAvailability(int productId, [FromBody] UpdateProductAvailabilityRequest request)
        {
            var product = await _productService.UpdateAvailabilityAsync(productId, request);
            return product is null ? NotFound() : Ok(product);
        }

        [HttpDelete("{productId:int}", Name = "DeleteProduct")]
        public async Task<IActionResult> Delete(int productId)
        {
            if (productId < 1) return BadRequest($"Not accepted ID {productId}");
            try
            {
                var deleted = await _productService.DeleteAsync(productId);
                return deleted ? NoContent() : NotFound();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return Conflict(new { message = "This product cannot be deleted because related data still exists." });
            }
        }
    }
}
