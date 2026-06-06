using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using ECommerecAPI.Services;
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
        public ApplicationDbContext _context;
        public EmailSending _emailService;
        public JwtService _jwt;

        public AuthController(ApplicationDbContext context, EmailSending emailService, JwtService jwt)
        {
            _context = context;
            _emailService = emailService;
            _jwt = jwt;
        }

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

            // Check unique email
            var existingUser = _context.Users
                .FirstOrDefault(u => u.email == userDto.email);

            if (existingUser != null)
            {
                return BadRequest("Email already exists.");
            }

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

            // Fixed: was referencing undefined `user`, should be `u`
            var token = _jwt.GenerateToken(u);

            return Ok(new
            {
                message = "Registration successful.",
                userId = u.Id,
                token
            });
        }

        [HttpPost("LoginUser")]
        public IActionResult LoginUser(string email, string password)
        {
            // Fixed: was referencing undefined `dto.Email` / `dto.Password`
            // Fixed: was using BCrypt but passwords are SHA256-hashed
            string hashedPassword = HashPassword(password);
            var user = _context.Users.FirstOrDefault(u =>
                u.email == email && u.password == hashedPassword);

            if (user == null)
                return Unauthorized("Invalid email or password.");

            var token = _jwt.GenerateToken(user);

            return Ok(token);
        }

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}
