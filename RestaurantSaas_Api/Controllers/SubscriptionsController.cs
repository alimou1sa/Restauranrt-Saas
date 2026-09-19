    using global::RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsRequest;
    using global::RestaurantSaaS.Application.DTOs.Subscriptions.SubscriptionsResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace RestaurantSaas_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpPost("organizations/{organizationId:int}/subscription", Name = "CreateSubscription")]
        public async Task<ActionResult<SubscriptionResponse>> Create(int organizationId,[FromBody] CreateSubscriptionRequest request)
        {
            if (organizationId <= 0)
                return BadRequest(new { message = "Organization ID must be greater than 0." });

            try
            {
                var subscription = await _subscriptionService.CreateAsync(organizationId, request);
                return CreatedAtRoute("GetCurrentSubscriptionByOrganization", new { organizationId }, subscription);
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

        [HttpGet("organizations/{organizationId:int}/subscription", Name = "GetCurrentSubscriptionByOrganization")]
        public async Task<ActionResult<SubscriptionResponse>> GetCurrentByOrganization(int organizationId)
        {
            if (organizationId <= 0)
                return BadRequest(new { message = "Organization ID must be greater than 0." });

            var subscription = await _subscriptionService.GetCurrentByOrganizationAsync(organizationId);

            return subscription is null
                ? NotFound(new { message = "This organization has no subscription." })
                : Ok(subscription);
        }

        [HttpPatch("subscriptions/{subscriptionId:int}/plan", Name = "ChangeSubscriptionPlan")]
        public async Task<ActionResult<SubscriptionResponse>> ChangePlan(int subscriptionId,[FromBody] ChangeSubscriptionPlanRequest request)
        {
            if (subscriptionId <= 0)
                return BadRequest(new { message = "Subscription ID must be greater than 0." });

            try
            {
                var subscription = await _subscriptionService.ChangePlanAsync(subscriptionId, request);

                return subscription is null
                    ? NotFound(new { message = "Subscription not found." })
                    : Ok(subscription);
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

        [HttpPatch("subscriptions/{subscriptionId:int}/cancel", Name = "CancelSubscription")]
        public async Task<ActionResult<SubscriptionResponse>> Cancel(int subscriptionId,[FromBody] CancelSubscriptionRequest request)
        {
            if (subscriptionId <= 0)
                return BadRequest(new { message = "Subscription ID must be greater than 0." });

            var subscription = await _subscriptionService.CancelAsync(subscriptionId, request);

            return subscription is null
                ? NotFound(new { message = "Subscription not found." })
                : Ok(subscription);
        }
    }
}
