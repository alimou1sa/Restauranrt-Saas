    using global::RestaurantSaaS.Application.DTOs.Users.UserRequest;
    using global::RestaurantSaaS.Application.DTOs.Users.UserResponse;
    using global::RestaurantSaaS.Application.InterfacesService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
namespace RestaurantSaas_Api.Controllers
{
   // [Authorize]
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost(Name = "CreateUser")]
        public async Task<ActionResult<UserResponse>> Create([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = await _userService.CreateAsync(request);

                return CreatedAtRoute("GetUserById",new { userId = user.UserId },user);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }

  
        [HttpGet("{userId:int}", Name = "GetUserById")]
        public async Task<ActionResult<UserResponse>> GetById(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new
                {
                    message = "User ID must be greater than 0."
                });
            }

            var user = await _userService.GetByIdAsync(userId);

            return user is null
                ? NotFound(new
                {
                    message = "User not found."
                })
                : Ok(user);
        }

   
        [HttpGet(Name = "GetAllUsers")]
        public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
        {
            var users = await _userService.GetAllAsync();

            return Ok(users);
        }

    
        [HttpPut("{userId:int}", Name = "UpdateUser")]
        public async Task<ActionResult<UserResponse>> Update(int userId,[FromBody] UpdateUserRequest request)
        {
            if (userId <= 0)
            {
                return BadRequest(new
                {
                    message = "User ID must be greater than 0."
                });
            }

            try
            {
                var user = await _userService.UpdateAsync(userId, request);

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

   
        [HttpPut("{userId:int}/password",Name = "ChangeUserPassword")]
        public async Task<IActionResult> ChangePassword(int userId,[FromBody] ChangePasswordRequest request)
        {
            if (userId <= 0)
            {
                return BadRequest(new
                {
                    message = "User ID must be greater than 0."
                });
            }

            try
            {
                var changed = await _userService.ChangePasswordAsync(
                    userId,
                    request);

                return changed
                    ? NoContent()
                    : NotFound(new
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

 
        [HttpDelete("{userId:int}", Name = "DeleteUser")]
        public async Task<IActionResult> Delete(int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new
                {
                    message = "User ID must be greater than 0."
                });
            }

            try
            {
                var deleted = await _userService.DeleteAsync(userId);

                return deleted
                    ? NoContent()
                    : NotFound(new
                    {
                        message = "User not found."
                    });
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "This user cannot be deleted because related data still exists."
                });
            }
        }
    }
}
