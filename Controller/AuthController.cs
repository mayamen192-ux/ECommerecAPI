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
            => _authService.RegisterUser(userDto);

        [HttpPost("LoginUser")]
        public IActionResult LoginUser([FromBody] LoginDTO dto)
            => _authService.LoginUser(dto);

        [AllowAnonymous]
        [HttpGet("DebugLogin")]
        public IActionResult DebugLogin(string email, string password)
            => _authService.DebugLogin(email, password);
    }
}