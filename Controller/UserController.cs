using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using ECommerecAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ECommerecAPI.Controller;

[ApiController]
[Route("api/[controller]")]
[Authorize]  // Default: all endpoints require authentication UNLESS overridden with [AllowAnonymous]
public class UserController : ControllerBase
{
    public ApplicationDbContext _context;
    public EmailSending _emailService;
    private readonly IConfiguration _configuration;

    public UserController(ApplicationDbContext context, EmailSending emailService, IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }

    //  [AllowAnonymous] — Registration must be publicly accessible
    [AllowAnonymous]
    [HttpPost("RegisterUser")]
    public IActionResult RegisterUser([FromBody] UserRegister userDto)
    {
        // Validate email format
        try
        {
            var email = new MailAddress(userDto.email);
        }
        catch
        {
            return BadRequest("Invalid email format.");
        }

        // Check for duplicate email
        var existingUser = _context.Users
            .FirstOrDefault(u => u.email == userDto.email);

        if (existingUser != null)
            return BadRequest("Email already exists.");

        // Create new user
        User u = new User
        {
            name = userDto.name,
            email = userDto.email,
            phone = userDto.phone,
            password = HashPassword(userDto.password),
            role = "User",
            createdAt = DateTime.Now
        };

        _context.Users.Add(u);
        _context.SaveChanges();

        _emailService.SendEmail(userDto.email, "Welcome! " + userDto.name, "Thank you for registering.");

        return Ok("User registered successfully with ID = " + u.Id);
    }

    //[AllowAnonymous] — Login must be publicly accessible
    //Returns a JWT token instead of the raw user object
    [AllowAnonymous]
    [HttpPost("LoginUser")]
    public IActionResult LoginUser(string email, string password)
    {
        try
        {
            string hashedPassword = HashPassword(password);

            var user = _context.Users.FirstOrDefault(u =>
                u.email == email &&
                u.password == hashedPassword);

            if (user == null)
                return NotFound("Invalid email or password.");

            // Generate JWT token
            string token = GenerateJwtToken(user);

            return Ok(new
            {
                token = token,
                userId = user.Id,
                name = user.name,
                role = user.role
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.ToString());
        }
    }

    // Admin-only access
    [Authorize(Roles = "Admin")]
    [HttpGet("GetAllUsers")]
    public IActionResult GetAllUsers()
    {
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

        return Ok(outputUsers);
    }

    // Authenticated users can only view their OWN data (Admins can view any)
    [Authorize]
    [HttpGet("GetUserById")]
    public IActionResult GetUserById(int id)
    {
        // Extract the requesting user's ID from their JWT token claims
        var requestingUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (requestingUserIdClaim == null)
            return Unauthorized("Invalid token.");

        int requestingUserId = int.Parse(requestingUserIdClaim);

        // Only allow users to access their own data; Admins can access any
        if (requestingUserId != id && !User.IsInRole("Admin"))
            return Forbid();

        var user = _context.Users.Find(id);

        if (user == null)
            return NotFound("User not found.");

        // Return a safe DTO (never expose hashed password)
        var output = new UserOutput
        {
            name = user.name,
            email = user.email,
            phone = user.phone
        };

        return Ok(output);
    }

    // --- Private Helpers ---

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.email),
            new Claim(ClaimTypes.Name, user.name),
            new Claim(ClaimTypes.Role, user.role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}