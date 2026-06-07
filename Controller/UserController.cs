using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using ECommerecAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ECommerecAPI.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly EmailSending _emailService;
    private readonly JwtService _jwt; //  use JwtService only
    private readonly ILogger<UserController> _logger;

    public UserController(
        ILogger<UserController> logger,
        ApplicationDbContext context,
        EmailSending emailService,
        JwtService jwt) // inject JwtService
    {
        _logger = logger;
        _context = context;
        _emailService = emailService;
        _jwt = jwt;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("GetAllUsers")]
    public IActionResult GetAllUsers()
    {
        _logger.LogInformation("Admin requested all users list");

        var users = _context.Users
                      .Include(u => u.orders)
                          .ThenInclude(o => o.OrderProducts)
                              .ThenInclude(op => op.Product)
                      .ToList();

        var outputUsers = users.Select(user => new UserOutput
        {
            name = user.name,
            email = user.email,
            phone = user.phone,
            orders = user.orders.Select(o => new OrderOutput
            {
                order_Id = o.order_Id,
                orderDate = o.orderDate,
                totalAmount = o.TotalAmount
            }).ToList()
        }).ToList();

        _logger.LogInformation("Returned {Count} users", outputUsers.Count);

        return Ok(outputUsers);
    }

    [Authorize]
    [HttpGet("GetUserById")]
    public IActionResult GetUserById(int id)
    {
        _logger.LogInformation("GetUserById called for ID: {Id}", id);

        var requestingUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (requestingUserIdClaim == null)
        {
            _logger.LogWarning("GetUserById failed - invalid token");
            return Unauthorized("Invalid token.");
        }

        int requestingUserId = int.Parse(requestingUserIdClaim);

        if (requestingUserId != id && !User.IsInRole("Admin"))
        {
            _logger.LogWarning("User {RequestingId} tried to access User {TargetId} - Forbidden",
                requestingUserId, id);
            return Forbid();
        }

        var user = _context.Users.Find(id);

        if (user == null)
        {
            _logger.LogWarning("User not found with ID: {Id}", id);
            return NotFound("User not found.");
        }

        _logger.LogInformation("User {Id} data returned successfully", id);

        return Ok(new UserOutput
        {
            name = user.name,
            email = user.email,
            phone = user.phone
        });
    }

    // Keep HashPassword only if needed elsewhere
   public  static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }
}