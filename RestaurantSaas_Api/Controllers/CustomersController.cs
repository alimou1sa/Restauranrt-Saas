    using global::RestaurantSaaS.Application.DTOs.Customers.CustomersRequest;
    using global::RestaurantSaaS.Application.DTOs.Customers.CustomersResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace RestaurantSaas_Api.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/Customers")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost("organizations/{organizationId:int}", Name = "CreateCustomer")]
        public async Task<ActionResult<CustomerDetailsResponse>> Create(int organizationId,[FromBody] CreateCustomerRequest request)
        {
            try
            {
                var customer = await _customerService.CreateAsync(organizationId, request);
                return CreatedAtRoute("GetCustomerById", new { customerId = customer.CustomerId }, customer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{customerId:int}", Name = "GetCustomerById")]
        public async Task<ActionResult<CustomerDetailsResponse>> GetById(int customerId)
        {
            var customer = await _customerService.GetByIdAsync(customerId);
            return customer is null ? NotFound() : Ok(customer);
        }

        [HttpGet("organizations/{organizationId:int}", Name = "GetCustomersByOrganization")]
        public async Task<ActionResult<IEnumerable<CustomerListResponse>>> GetAllByOrganization(int organizationId)
        {
            var customers = await _customerService.GetAllByOrganizationAsync(organizationId);
            return Ok(customers);
        }

        [HttpPut("{customerId:int}", Name = "UpdateCustomer")]
        public async Task<ActionResult<CustomerDetailsResponse>> Update(int customerId,[FromBody] UpdateCustomerRequest request)
        {
            var customer = await _customerService.UpdateAsync(customerId, request);
            return customer is null ? NotFound() : Ok(customer);
        }

        [HttpDelete("{customerId:int}", Name = "DeleteCustomer")]
        public async Task<IActionResult> Delete(int customerId)
        {
            try
            {
                var deleted = await _customerService.DeleteAsync(customerId);
                return deleted ? NoContent() : NotFound();
            }
            catch (DbUpdateException)
            {
                return Conflict(new { message = "This customer cannot be deleted because related data still exists." });
            }
        }
    }
}
