    using global::RestaurantSaaS.Application.DTOs.Branches.BranchRequest;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantSaaS.Infrastructure;
using RestaurantSaaS.Infrastructure.Auth;

namespace RestaurantSaas_Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Branches")]
    public class BranchesController : ControllerBase
    {
        private readonly IBranchService _branchService;

        public BranchesController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        [HasPermission("branch.manage")]
        [HttpPost( Name = "CreateBranch")]
        public async Task<ActionResult<BranchResponse>> Create([FromBody] CreateBranchRequest request)
        {
    
            try
            {
                var branch = await _branchService.CreateAsync(request);

                return CreatedAtRoute("GetBranchById",new { branchId = branch.BranchId },branch);

            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new{message = ex.Message});
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new{message = ex.Message});
            }
        }


        [HasPermission("branch.read")]
        [HttpGet("{branchId:int}",Name = "GetBranchById")]
        public async Task<ActionResult<BranchResponse>> GetById(int branchId)
        {
            if (branchId <= 0) return BadRequest(new{message = "Branch ID must be greater than 0."});

            var branch = await _branchService.GetByIdAsync(branchId);

            return branch is null? NotFound(new{message = "Branch not found."}): Ok(branch);
        }

        [HasPermission("branch.read")]
        [HttpGet(Name = "GetBranches")]
        public async Task<ActionResult<IEnumerable<BranchResponse>>> GetBranches()
        {

            var branches = await _branchService.GetAllAsync();

            return Ok(branches);
        }

        [HasPermission("branch.manage")]
        [HttpPut("{branchId:int}",Name = "UpdateBranch")]
        public async Task<ActionResult<BranchResponse>> Update(int branchId,[FromBody] UpdateBranchRequest request)
        {
            if (branchId <= 0)
                return BadRequest(new
                {
                    message = "Branch ID must be greater than 0."
                });

            try
            {
                var branch = await _branchService.UpdateAsync(branchId,request);

                return branch is null? NotFound(new{message = "Branch not found."}): Ok(branch);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

        [HasPermission("branch.manage")]
        [HttpDelete("{branchId:int}",Name = "DeleteBranch")]
        public async Task<IActionResult> Delete(int branchId)
        {
            if (branchId <= 0)return BadRequest(new
                {
                    message = "Branch ID must be greater than 0."
                });

            try
            {
                var deleted = await _branchService.DeleteAsync(branchId);

                return deleted? NoContent(): NotFound(new
                    {
                        message = "Branch not found."
                    });
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "This branch cannot be deleted because related data still exists."
                });
            }
        }
    }
}