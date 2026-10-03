    using global::RestaurantSaaS.Application.DTOs.Orders.OrdersRequest;
    using global::RestaurantSaaS.Application.DTOs.Orders.OrdersResponse;
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
    [Route("api/Orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HasPermission("order.manage")]
        [HttpPost("branches/{branchId:int}", Name = "CreateOrder")]
        public async Task<ActionResult<OrderDetailsResponse>> Create(int branchId,[FromBody] CreateOrderRequest request)
        {
            if (branchId <= 0)
                return BadRequest(new { message = "Branch ID must be greater than 0." });

            try
            {
                var order = await _orderService.CreateAsync(branchId, request);
                return CreatedAtRoute("GetOrderById", new { orderId = order.OrderId }, order);
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

        [HasPermission("order.read")]
        [HttpGet("{orderId:int}", Name = "GetOrderById")]
        public async Task<ActionResult<OrderDetailsResponse>> GetById(int orderId)
        {
            if (orderId <= 0)
                return BadRequest(new { message = "Order ID must be greater than 0." });

            var order = await _orderService.GetByIdAsync(orderId);

            return order is null
                ? NotFound(new { message = "Order not found." })
                : Ok(order);
        }

        [HasPermission("order.read")]
        [HttpGet( Name = "GetOrdersByOrganization")]
        public async Task<ActionResult<IEnumerable<OrderListResponse>>> GetOrders()
        {


            var orders = await _orderService.GetAllAsync();
            return Ok(orders);
        }

        [HasPermission("order.manage")]
        [HttpPut("{orderId:int}", Name = "UpdateOrder")]
        public async Task<ActionResult<OrderDetailsResponse>> Update(int orderId,[FromBody] UpdateOrderRequest request)
        {
            if (orderId <= 0)
                return BadRequest(new { message = "Order ID must be greater than 0." });

            try
            {
                var order = await _orderService.UpdateAsync(orderId, request);

                return order is null
                    ? NotFound(new { message = "Order not found." })
                    : Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HasPermission("order.manage")]
        [HttpPatch("orders/{orderId:int}/status", Name = "UpdateOrderStatus")]
        public async Task<ActionResult<OrderDetailsResponse>> UpdateStatus(int orderId,[FromBody] UpdateOrderStatusRequest request)
        {
            if (orderId <= 0)
                return BadRequest(new { message = "Order ID must be greater than 0." });

            try
            {
                var order = await _orderService.UpdateStatusAsync(orderId, request);

                return order is null
                    ? NotFound(new { message = "Order not found." })
                    : Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HasPermission("order.manage")]
        [HttpDelete("orders/{orderId:int}", Name = "DeleteOrder")]
        public async Task<IActionResult> Delete(int orderId)
        {
            if (orderId <= 0)
                return BadRequest(new { message = "Order ID must be greater than 0." });

            try
            {
                var deleted = await _orderService.DeleteAsync(orderId);

                return deleted
                    ? NoContent()
                    : NotFound(new { message = "Order not found." });
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "This order cannot be deleted because related data (items/payments) still exists." });
            }
        }
    }
}
