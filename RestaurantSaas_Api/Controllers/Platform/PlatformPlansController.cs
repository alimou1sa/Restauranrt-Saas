using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantSaaS.Application.DTOs.Plans.PlansRequest;
using RestaurantSaaS.Application.DTOs.Plans.PlansResponse;
using RestaurantSaaS.Application.InterfacesService;

namespace RestaurantSaas_Api.Controllers.Platform
{
    // Controllers/Platform/PlatformPlansController.cs — يعيد استخدام IPlanService الموجود كما هو، بلا أي تعديل عليه
    [Authorize(Policy = "PlatformAdmin")]
    [ApiController]
    [Route("api/platform/plans")]
    public class PlatformPlansController : ControllerBase
    {
        private readonly IPlanService _planService;
        public PlatformPlansController(IPlanService planService) => _planService = planService;

        [HttpPost]
        public async Task<ActionResult<PlanResponse>> Create([FromBody] CreatePlanRequest request)
        {
            try { return Ok(await _planService.CreateAsync(request)); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpGet] public async Task<ActionResult<List<PlanResponse>>> GetAll() => Ok(await _planService.GetAllAsync());

        [HttpPut("{planId:int}")]
        public async Task<ActionResult<PlanResponse>> Update(int planId, [FromBody] UpdatePlanRequest request)
        {
            try
            {
                var result = await _planService.UpdateAsync(planId, request);
                return result is null ? NotFound() : Ok(result);
            }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        }

        [HttpDelete("{planId:int}")]
        public async Task<IActionResult> Delete(int planId)
            => await _planService.DeleteAsync(planId) ? NoContent() : NotFound();
    }
}
