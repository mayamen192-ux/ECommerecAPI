using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using ECommerecAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailSending _emailService;
        private readonly JwtService _jwt;  // only use JwtService — no private GenerateJwtToken
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ILogger<AuthController> logger,
            ApplicationDbContext context,
            EmailSending emailService,
            JwtService jwt)  //  inject JwtService only — no IConfiguration needed
        {
            _logger = logger;
            _context = context;
            _emailService = emailService;
            _jwt = jwt;
        }

        [AllowAnonymous]
        [HttpPost("RegisterUser")]
        public IActionResult RegisterUser([FromBody] UserRegister userDto)
        {
            _logger.LogInformation("Register attempt for email: {Email}", userDto.email);

            try
            {
                var email = new MailAddress(userDto.email);
            }
            catch
            {
                _logger.LogWarning("Invalid email format: {Email}", userDto.email);
                return BadRequest("Invalid email format.");
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.email == userDto.email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed - email already exists: {Email}", userDto.email);
                return BadRequest("Email already exists.");
            }

            User u = new User
            {
                name = userDto.name,
                email = userDto.email,
                phone = userDto.phone,
                password = HashPassword(userDto.password),
                role = "User",
                IsActive = true,  // activate on register
                createdAt = DateTime.Now
            };

            _context.Users.Add(u);
            _context.SaveChanges();

            _logger.LogInformation("User registered successfully: {Email} with ID: {Id}", u.email, u.Id);

            _emailService.SendEmail(userDto.email, "Welcome! " + userDto.name, "Thank you for registering.");

            // use JwtService — same as LoginUser
            var token = _jwt.GenerateToken(u);

            return Ok(new
            {
                message = "Registration successful.",
                userId = u.Id,
                token
            });
        }

        [AllowAnonymous]
        [HttpPost("LoginUser")]
        public IActionResult LoginUser([FromBody] LoginDTO dto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login failed - invalid model state for: {Email}", dto.Email);
                return BadRequest(ModelState);
            }

            try
            {
                string hashedPassword = HashPassword(dto.Password);

                var user = _context.Users.FirstOrDefault(u =>
                    u.email == dto.Email &&
                    u.password == hashedPassword);

                if (user == null)
                {
                    _logger.LogWarning("Login failed - invalid credentials for: {Email}", dto.Email);
                    return Unauthorized("Invalid email or password.");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed - account not activated for: {Email}", dto.Email);
                    return Unauthorized("Account is not activated. Please check your email.");
                }

                // use JwtService — same key/issuer/audience as Program.cs validation
                var token = _jwt.GenerateToken(user);

                _logger.LogInformation("Login successful for: {Email} | Role: {Role}", user.email, user.role);

                return Ok(new
                {
                    token,
                    userId = user.Id,
                    name = user.name,
                    role = user.role
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for: {Email}", dto.Email);
                return BadRequest(ex.Message);
            }
        }

        // Keep for debugging — remove after fixing
        [AllowAnonymous]
        [HttpGet("DebugHash")]
        public IActionResult DebugHash(string password)
        {
            return Ok(HashPassword(password));
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
     
        [AllowAnonymous]
        [HttpGet("DebugConfig")]
        public IActionResult DebugConfig()
        {
            return Ok(new
            {
                message = "If you see this, Auth controller is working",
                time = DateTime.UtcNow
            });
        }
    }
    }
