using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace ECommerecAPI.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailSending _emailService;
        private readonly JwtService _jwt;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ILogger<AuthService> logger,
            ApplicationDbContext context,
            EmailSending emailService,
            JwtService jwt)
        {
            _logger = logger;
            _context = context;
            _emailService = emailService;
            _jwt = jwt;
        }

        public IActionResult RegisterUser(UserRegister userDto)
        {
            _logger.LogInformation("Register attempt for email: {Email}", userDto.email);

            try { var email = new MailAddress(userDto.email); }
            catch
            {
                _logger.LogWarning("Invalid email format: {Email}", userDto.email);
                return new BadRequestObjectResult("Invalid email format.");
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.email == userDto.email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed - email already exists: {Email}", userDto.email);
                return new BadRequestObjectResult("Email already exists.");
            }

            var u = new User
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

            _logger.LogInformation("User registered successfully: {Email} with ID: {Id}", u.email, u.Id);

            var token = _jwt.GenerateToken(u);

            _logger.LogInformation("Token generated for new user: {Email}", u.email);

            return new OkObjectResult(new
            {
                message = "Registration successful.",
                userId = u.Id,
                token
            });
        }

        public IActionResult LoginUser(LoginDTO dto)
        {
            _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

            try
            {
                string hashedPassword = HashPassword(dto.Password);

                var user = _context.Users.FirstOrDefault(u =>
                    u.email == dto.Email && u.password == hashedPassword);

                if (user == null)
                {
                    _logger.LogWarning("Login failed - invalid credentials for: {Email}", dto.Email);
                    return new UnauthorizedObjectResult("Invalid email or password.");
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed - account not activated for: {Email}", dto.Email);
                    return new UnauthorizedObjectResult("Account is not activated. Please check your email.");
                }

                var token = _jwt.GenerateToken(user);

                _logger.LogInformation("Login successful for: {Email} | Role: {Role}", user.email, user.role);

                return new OkObjectResult(new { token, role = user.role, name = user.name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for: {Email}", dto.Email);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        public IActionResult DebugLogin(string email, string password)
        {
            string hashedPassword = HashPassword(password);

            var user = _context.Users.FirstOrDefault(u => u.email == email);

            if (user == null)
                return new NotFoundObjectResult(new { message = "User not found in DB", email });

            return new OkObjectResult(new
            {
                emailMatch = user.email == email,
                passwordMatch = user.password == hashedPassword,
                isActive = user.IsActive,
                storedHash = user.password,
                inputHash = hashedPassword
            });
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}