using ECommerecAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            var result = _userService.GetAllUsers();
            return ToActionResult(result);
        }

        [Authorize]
        [HttpGet("GetUserById")]
        public IActionResult GetUserById(int id)
        {
            var result = _userService.GetUserById(id, User);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult(object result)
        {
            var statusCode = (int)result.GetType().GetProperty("statusCode")!.GetValue(result)!;
            return statusCode switch
            {
                200 => Ok(result),
                400 => BadRequest(result),
                401 => Unauthorized(result),
                403 => Forbid(),
                404 => NotFound(result),
                _ => StatusCode(statusCode, result)
            };
        }
    }
}