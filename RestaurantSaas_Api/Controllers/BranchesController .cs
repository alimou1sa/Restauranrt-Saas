    using global::RestaurantSaaS.Application.DTOs.Branches.BranchRequest;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RestaurantSaas_Api.Controllers
{
   // [Authorize]
    [ApiController]
    [Route("api/Branches")]
    public class BranchesController : ControllerBase
    {
        private readonly IBranchService _branchService;

        public BranchesController(IBranchService branchService)
        {
            _branchService = branchService;
        }


        [HttpPost("organizations/{organizationId:int}",Name = "CreateBranch")]
        public async Task<ActionResult<BranchResponse>> Create(int organizationId,[FromBody] CreateBranchRequest request)
        {
            if (organizationId <= 0)
                return BadRequest(new{message = "Organization ID must be greater than 0."});

            try
            {
                var branch = await _branchService.CreateAsync(organizationId,request);

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

     
        [HttpGet("{branchId:int}",Name = "GetBranchById")]
        public async Task<ActionResult<BranchResponse>> GetById(int branchId)
        {
            if (branchId <= 0) return BadRequest(new{message = "Branch ID must be greater than 0."});

            var branch = await _branchService.GetByIdAsync(branchId);

            return branch is null? NotFound(new{message = "Branch not found."}): Ok(branch);
        }

      
        [HttpGet("organizations/{organizationId:int}",Name = "GetBranchesByOrganization")]
        public async Task<ActionResult<IEnumerable<BranchResponse>>> GetAllByOrganization(int organizationId)
        {
            if (organizationId <= 0)
                return BadRequest(new
                {
                    message = "Organization ID must be greater than 0."
                });

            var branches = await _branchService.GetAllByOrganizationAsync(organizationId);

            return Ok(branches);
        }

   
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