using ECommerecAPI.DTOs;
using ECommerecAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("RegisterUser")]
        public IActionResult RegisterUser([FromBody] UserRegister userDto)
        {
            var result = _authService.RegisterUser(userDto);
            return ToActionResult(result);
        }

        [HttpPost("LoginUser")]
        public IActionResult LoginUser([FromBody] LoginDTO dto)
        {
            var result = _authService.LoginUser(dto);
            return ToActionResult(result);
        }

        [AllowAnonymous]
        [HttpGet("DebugLogin")]
        public IActionResult DebugLogin(string email, string password)
        {
            var result = _authService.DebugLogin(email, password);
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
                404 => NotFound(result),
                403 => Forbid(),
                _ => StatusCode(statusCode, result)
            };
        }
    }
}