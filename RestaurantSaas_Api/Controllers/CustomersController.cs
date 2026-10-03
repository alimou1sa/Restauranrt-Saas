    using global::RestaurantSaaS.Application.DTOs.Customers.CustomersRequest;
    using global::RestaurantSaaS.Application.DTOs.Customers.CustomersResponse;
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
    [Route("api/Customers")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }
        [HasPermission("customer.manage")]
        [HttpPost( Name = "CreateCustomer")]
        public async Task<ActionResult<CustomerDetailsResponse>> CreateCustomer([FromBody] CreateCustomerRequest request)
        {
            try
            {
                var customer = await _customerService.CreateAsync( request);
                return CreatedAtRoute("GetCustomerById", new { customerId = customer.CustomerId }, customer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HasPermission("customer.read")]
        [HttpGet("{customerId:int}", Name = "GetCustomerById")]
        public async Task<ActionResult<CustomerDetailsResponse>> GetById(int customerId)
        {
            var customer = await _customerService.GetByIdAsync(customerId);
            return customer is null ? NotFound() : Ok(customer);
        }

        [HasPermission("customer.read")]
        [HttpGet( Name = "GetCustomers")]
        public async Task<ActionResult<IEnumerable<CustomerListResponse>>> GetCustomers()
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(customers);
        }

        [HasPermission("customer.manage")]
        [HttpPut("{customerId:int}", Name = "UpdateCustomer")]
        public async Task<ActionResult<CustomerDetailsResponse>> Update(int customerId,[FromBody] UpdateCustomerRequest request)
        {
            var customer = await _customerService.UpdateAsync(customerId, request);
            return customer is null ? NotFound() : Ok(customer);
        }

        [HasPermission("customer.manage")]
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
