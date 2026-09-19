    using global::RestaurantSaaS.Application.DTOs.RestaurantTables.RestaurantTablesRequest;
    using global::RestaurantSaaS.Application.DTOs.RestaurantTables.RestaurantTablesResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RestaurantSaaS.Infrastructure;
namespace RestaurantSaas_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class RestaurantTablesController : ControllerBase
    {
        private readonly IRestaurantTableService _restaurantTableService;

        public RestaurantTablesController(IRestaurantTableService restaurantTableService)
        {
            _restaurantTableService = restaurantTableService;
        }

        [HttpPost("api/branches/{branchId:int}/tables" , Name = "CreateRestaurantTable")]
        public async Task<ActionResult<RestaurantTableResponse>> Create(int branchId, [FromBody] CreateRestaurantTableRequest request)
        {
            if (branchId < 1) return BadRequest($"Not accepted ID {branchId}");
            try
            {
                var table = await _restaurantTableService.CreateAsync(branchId, request);
                return CreatedAtRoute("GetRestaurantTableByid", new { tableId = table.TableId }, table);
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

        [HttpGet("api/tables/{tableId:int}", Name = "GetRestaurantTableByid")]
        public async Task<ActionResult<RestaurantTableResponse>> GetById(int tableId)
        {
            if (tableId < 1) return BadRequest($"Not accepted ID {tableId}");

            var table = await _restaurantTableService.GetByIdAsync(tableId);
            return table is null ? NotFound() : Ok(table);
        }

        [HttpGet("api/branches/{branchId:int}/tables", Name = "GetAllRestaurantTable")]
        public async Task<ActionResult<IEnumerable<RestaurantTableResponse>>> GetAllByBranch(int branchId)
        {
            var tables = await _restaurantTableService.GetAllByBranchAsync(branchId);
            return Ok(tables);
        }

        [HttpPut("api/tables/{tableId:int}", Name = "UpdateRestaurantTable")]
        public async Task<ActionResult<RestaurantTableResponse>> Update(int tableId, [FromBody] UpdateRestaurantTableRequest request)
        {
            if (tableId < 1) return BadRequest($"Not accepted ID {tableId}");
            try
            {
                var table = await _restaurantTableService.UpdateAsync(tableId, request);
                return table is null ? NotFound() : Ok(table);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("api/tables/{tableId:int}", Name = "DeleteRestaurantTable")]
        public async Task<IActionResult> Delete(int tableId)
        {
            if (tableId < 1) return BadRequest($"Not accepted ID {tableId}");
            try
            {
                var deleted = await _restaurantTableService.DeleteAsync(tableId);
                return deleted ? NoContent() : NotFound();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return Conflict(new { message = "This table cannot be deleted because related data still exists." });
            }
        }
    }
}
