    using global::RestaurantSaaS.Application.DTOs.Users.UserRequest;
    using global::RestaurantSaaS.Application.DTOs.Users.UserResponse;
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
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HasPermission("user.manage")]
              [HttpPost(Name = "CreateUser")]
              public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request)
              {
                  try
                  {
                      var user = await _userService.CreateAsync(request);

                return Ok(user);
      
                  }
                  catch (InvalidOperationException ex)
                  {
                      return Conflict(new
                      {
                          message = ex.Message
                      });
                  }
              }




        [HttpGet("me", Name = "GetMyProfile")]
        public async Task<ActionResult<UserResponse>> GetMyProfile()
        {
            var user = await _userService.GetMyProfileAsync();

            return user is null
                ? NotFound(new { message = "User not found." })
                : Ok(user);
        }

        [HttpPut( Name = "UpdateUser")]
        public async Task<ActionResult<UserResponse>> UpdateMe([FromBody] UpdateUserRequest request)
        {
    

            try
            {
                var user = await _userService.UpdateMyProfileAsync(request);

                return user is null
                    ? NotFound(new
                    {
                        message = "User not found."
                    })
                    : Ok(user);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

   
        [HttpPut("password",Name = "ChangeMyPassword")]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordRequest request)
        {

            try
            {
                var changed = await _userService.ChangePasswordAsync(request);

                return changed
                    ? NoContent(): NotFound(new
                    {
                        message = "User not found."
                    });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    message = ex.Message
                });
            }
        }

    }
}
