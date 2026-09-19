    using global::RestaurantSaaS.Application.DTOs.Inventories.InventoriesRequest;
    using global::RestaurantSaaS.Application.DTOs.Inventories.InventoriesResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace RestaurantSaas_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class InventoriesController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoriesController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpPost("branches/{branchId:int}/inventories", Name = "CreateInventory")]
        public async Task<ActionResult<InventoryResponse>> Create(
            int branchId,
            [FromBody] CreateInventoryRequest request)
        {
            if (branchId <= 0)
                return BadRequest(new { message = "Branch ID must be greater than 0." });

            try
            {
                var inventory = await _inventoryService.CreateAsync(branchId, request);
                return CreatedAtRoute("GetInventoryById", new { inventoryId = inventory.InventoryId }, inventory);
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

        [HttpGet("inventories/{inventoryId:int}", Name = "GetInventoryById")]
        public async Task<ActionResult<InventoryResponse>> GetById(int inventoryId)
        {
            if (inventoryId <= 0)
                return BadRequest(new { message = "Inventory ID must be greater than 0." });

            var inventory = await _inventoryService.GetByIdAsync(inventoryId);

            return inventory is null
                ? NotFound(new { message = "Inventory record not found." })
                : Ok(inventory);
        }

        [HttpGet("branches/{branchId:int}/inventories", Name = "GetInventoriesByBranch")]
        public async Task<ActionResult<IEnumerable<InventoryResponse>>> GetAllByBranch(int branchId)
        {
            if (branchId <= 0)
                return BadRequest(new { message = "Branch ID must be greater than 0." });

            var inventories = await _inventoryService.GetAllByBranchAsync(branchId);
            return Ok(inventories);
        }

        [HttpPut("inventories/{inventoryId:int}/settings", Name = "UpdateInventorySettings")]
        public async Task<ActionResult<InventoryResponse>> UpdateSettings(
            int inventoryId,
            [FromBody] UpdateInventorySettingsRequest request)
        {
            if (inventoryId <= 0)
                return BadRequest(new { message = "Inventory ID must be greater than 0." });

            var inventory = await _inventoryService.UpdateSettingsAsync(inventoryId, request);

            return inventory is null
                ? NotFound(new { message = "Inventory record not found." })
                : Ok(inventory);
        }

        [HttpDelete("inventories/{inventoryId:int}", Name = "DeleteInventory")]
        public async Task<IActionResult> Delete(int inventoryId)
        {
            if (inventoryId <= 0)
                return BadRequest(new { message = "Inventory ID must be greater than 0." });

            try
            {
                var deleted = await _inventoryService.DeleteAsync(inventoryId);

                return deleted
                    ? NoContent()
                    : NotFound(new { message = "Inventory record not found." });
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "This inventory record cannot be deleted because related data still exists." });
            }
        }
    }
}
