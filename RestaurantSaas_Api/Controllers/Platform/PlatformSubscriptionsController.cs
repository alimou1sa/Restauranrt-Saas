using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsRequest;
using RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsResponse;
using RestaurantSaaS.Application.InterfacesService;

namespace RestaurantSaas_Api.Controllers.Platform
{
    // Controllers/Platform/PlatformSubscriptionsController.cs
    [Authorize(Policy = "PlatformAdmin")]
    [ApiController]
    [Route("api/platform/subscriptions")]
    public class PlatformSubscriptionsController : ControllerBase
    {
        private readonly IPlatformSubscriptionService _service;
        public PlatformSubscriptionsController(IPlatformSubscriptionService service) => _service = service;

        [HttpGet] public async Task<ActionResult<List<SubscriptionResponse>>> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("organizations/{organizationId:int}")]
        public async Task<ActionResult<List<SubscriptionResponse>>> GetByOrganization(int organizationId)
            => Ok(await _service.GetByOrganizationAsync(organizationId));

        [HttpPatch("{subscriptionId:int}/plan")]
        public async Task<ActionResult<SubscriptionResponse>> ChangePlan(int subscriptionId, [FromBody] ChangeSubscriptionPlanRequest request)
        {
            try
            {
                var result = await _service.ChangePlanAsync(subscriptionId, request);
                return result is null ? NotFound() : Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPatch("{subscriptionId:int}/cancel")]
        public async Task<ActionResult<SubscriptionResponse>> Cancel(int subscriptionId, [FromBody] CancelSubscriptionRequest request)
        {
            var result = await _service.CancelAsync(subscriptionId, request);
            return result is null ? NotFound() : Ok(result);
        }
    }
}
