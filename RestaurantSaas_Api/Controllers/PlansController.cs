    using global::RestaurantSaaS.Application.DTOs.Plans.PlansRequest;
    using global::RestaurantSaaS.Application.DTOs.Plans.PlansResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace RestaurantSaas_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/plans")]
    public class PlansController : ControllerBase
    {
        private readonly IPlanService _planService;

        public PlansController(IPlanService planService)
        {
            _planService = planService;
        }

        [HttpPost(Name = "CreatePlan")]
        public async Task<ActionResult<PlanResponse>> Create([FromBody] CreatePlanRequest request)
        {
            try
            {
                var plan = await _planService.CreateAsync(request);
                return CreatedAtRoute("GetPlanById", new { planId = plan.PlanId }, plan);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("{planId:int}", Name = "GetPlanById")]
        public async Task<ActionResult<PlanResponse>> GetById(int planId)
        {
            if (planId <= 0)
                return BadRequest(new { message = "Plan ID must be greater than 0." });

            var plan = await _planService.GetByIdAsync(planId);

            return plan is null
                ? NotFound(new { message = "Plan not found." })
                : Ok(plan);
        }

        [HttpGet(Name = "GetAllPlans")]
        public async Task<ActionResult<IEnumerable<PlanResponse>>> GetAll()
        {
            var plans = await _planService.GetAllAsync();
            return Ok(plans);
        }

        [HttpPut("{planId:int}", Name = "UpdatePlan")]
        public async Task<ActionResult<PlanResponse>> Update(
            int planId,
            [FromBody] UpdatePlanRequest request)
        {
            if (planId <= 0)
                return BadRequest(new { message = "Plan ID must be greater than 0." });

            try
            {
                var plan = await _planService.UpdateAsync(planId, request);

                return plan is null
                    ? NotFound(new { message = "Plan not found." })
                    : Ok(plan);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpDelete("{planId:int}", Name = "DeletePlan")]
        public async Task<IActionResult> Delete(int planId)
        {
            if (planId <= 0)
                return BadRequest(new { message = "Plan ID must be greater than 0." });

            try
            {
                var deleted = await _planService.DeleteAsync(planId);

                return deleted
                    ? NoContent()
                    : NotFound(new { message = "Plan not found." });
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "This plan cannot be deleted because related data still exists." });
            }
        }
    }
}
