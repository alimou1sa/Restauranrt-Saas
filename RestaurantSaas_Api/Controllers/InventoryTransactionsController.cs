    using global::RestaurantSaaS.Application.DTOs.InventoryTransactions.InventoryTransactionsRequest;
    using global::RestaurantSaaS.Application.DTOs.InventoryTransactions.InventoryTransactionsResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace RestaurantSaas_Api.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/InventoryTransactions")]
    public class InventoryTransactionsController : ControllerBase
    {
        private readonly IInventoryTransactionService _inventoryTransactionService;

        public InventoryTransactionsController(IInventoryTransactionService inventoryTransactionService)
        {
            _inventoryTransactionService = inventoryTransactionService;
        }

        [HttpPost("inventories/{inventoryId:int}", Name = "CreateInventoryTransaction")]
        public async Task<ActionResult<InventoryTransactionResponse>> Create(int inventoryId,[FromBody] CreateInventoryTransactionRequest request)
        {
            if (inventoryId <= 0)
                return BadRequest(new { message = "Inventory ID must be greater than 0." });

            try
            {
                var transaction = await _inventoryTransactionService.CreateAsync(inventoryId, request);

                return StatusCode(StatusCodes.Status201Created, transaction);
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

        [HttpGet("branches/{branchId:int}", Name = "GetInventoryTransactionsByBranch")]
        public async Task<ActionResult<IEnumerable<InventoryTransactionResponse>>> GetAllByBranch(int branchId)
        {
            if (branchId <= 0)
                return BadRequest(new { message = "Branch ID must be greater than 0." });

            var transactions = await _inventoryTransactionService.GetAllByBranchAsync(branchId);
            return Ok(transactions);
        }
    }
}
