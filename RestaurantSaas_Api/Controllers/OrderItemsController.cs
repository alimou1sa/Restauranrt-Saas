    using global::RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsRequest;
    using global::RestaurantSaaS.Application.DTOs.OrderItems.OrderItemsResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace RestaurantSaas_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/OrderItems")]
    public class OrderItemsController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;

        public OrderItemsController(IOrderItemService orderItemService)
        {
            _orderItemService = orderItemService;
        }

        [HttpPost("orders/{orderId:int}", Name = "AddOrderItem")]
        public async Task<ActionResult<OrderItemResponse>> Add(int orderId,[FromBody] CreateOrderItemRequest request)
        {
            if (orderId <= 0)
                return BadRequest(new { message = "Order ID must be greater than 0." });

            try
            {
                var item = await _orderItemService.AddAsync(orderId, request);
                return CreatedAtRoute("GetOrderItemsByOrder", new { orderId }, item);
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

        [HttpGet("orders/{orderId:int}", Name = "GetOrderItemsByOrder")]
        public async Task<ActionResult<IEnumerable<OrderItemResponse>>> GetAllByOrder(int orderId)
        {
            if (orderId <= 0)
                return BadRequest(new { message = "Order ID must be greater than 0." });

            var items = await _orderItemService.GetAllByOrderAsync(orderId);
            return Ok(items);
        }

        [HttpPut("{orderItemId:int}", Name = "UpdateOrderItem")]
        public async Task<ActionResult<OrderItemResponse>> Update(int orderItemId,[FromBody] UpdateOrderItemRequest request)
        {
            if (orderItemId <= 0)
                return BadRequest(new { message = "Order item ID must be greater than 0." });

            var item = await _orderItemService.UpdateAsync(orderItemId, request);

            return item is null
                ? NotFound(new { message = "Order item not found." })
                : Ok(item);
        }

        [HttpDelete("{orderItemId:int}", Name = "RemoveOrderItem")]
        public async Task<IActionResult> Remove(int orderItemId)
        {
            if (orderItemId <= 0)
                return BadRequest(new { message = "Order item ID must be greater than 0." });

            var removed = await _orderItemService.RemoveAsync(orderItemId);

            return removed
                ? NoContent()
                : NotFound(new { message = "Order item not found." });
        }
    }
}
