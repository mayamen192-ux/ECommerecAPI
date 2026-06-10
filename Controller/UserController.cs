using ECommerecAPI.DTOs;
using ECommerecAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller;

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
        => _userService.GetAllUsers();

    [Authorize]
    [HttpGet("GetUserById")]
    public IActionResult GetUserById(int id)
        => _userService.GetUserById(id, User);
}