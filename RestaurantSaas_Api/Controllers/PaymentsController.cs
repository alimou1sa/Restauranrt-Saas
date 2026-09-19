    using global::RestaurantSaaS.Application.DTOs.Payments;
    using global::RestaurantSaaS.Application.DTOs.Payments.PaymentsRequest;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace RestaurantSaas_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("orders/{orderId:int}/payments", Name = "CreatePayment")]
        public async Task<ActionResult<PaymentResponse>> Create(
            int orderId,
            [FromBody] CreatePaymentRequest request)
        {
            if (orderId <= 0)
                return BadRequest(new { message = "Order ID must be greater than 0." });

            try
            {
                var payment = await _paymentService.CreateAsync(orderId, request);
                return CreatedAtRoute("GetPaymentById", new { paymentId = payment.PaymentId }, payment);
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

        [HttpGet("payments/{paymentId:int}", Name = "GetPaymentById")]
        public async Task<ActionResult<PaymentResponse>> GetById(int paymentId)
        {
            if (paymentId <= 0)
                return BadRequest(new { message = "Payment ID must be greater than 0." });

            var payment = await _paymentService.GetByIdAsync(paymentId);

            return payment is null
                ? NotFound(new { message = "Payment not found." })
                : Ok(payment);
        }

        [HttpGet("orders/{orderId:int}/payments", Name = "GetPaymentsByOrder")]
        public async Task<ActionResult<IEnumerable<PaymentResponse>>> GetAllByOrder(int orderId)
        {
            if (orderId <= 0)
                return BadRequest(new { message = "Order ID must be greater than 0." });

            var payments = await _paymentService.GetAllByOrderAsync(orderId);
            return Ok(payments);
        }

        [HttpPatch("payments/{paymentId:int}/status", Name = "UpdatePaymentStatus")]
        public async Task<ActionResult<PaymentResponse>> UpdateStatus(
            int paymentId,
            [FromBody] UpdatePaymentStatusRequest request)
        {
            if (paymentId <= 0)
                return BadRequest(new { message = "Payment ID must be greater than 0." });

            try
            {
                var payment = await _paymentService.UpdateStatusAsync(paymentId, request);

                return payment is null
                    ? NotFound(new { message = "Payment not found." })
                    : Ok(payment);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
