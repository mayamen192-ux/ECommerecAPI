using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ECommerecAPI.Services
{
    public class UserService
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailSending _emailService;
        private readonly JwtService _jwt;
        private readonly ILogger<UserService> _logger;

        public UserService(
            ILogger<UserService> logger,
            ApplicationDbContext context,
            EmailSending emailService,
            JwtService jwt)
        {
            _logger = logger;
            _context = context;
            _emailService = emailService;
            _jwt = jwt;
        }

        public object GetAllUsers()
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

            return new { statusCode = 200, data = outputUsers };
        }

        public object GetUserById(int id, ClaimsPrincipal currentUser)
        {
            _logger.LogInformation("GetUserById called for ID: {Id}", id);

            var requestingUserIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (requestingUserIdClaim == null)
            {
                _logger.LogWarning("GetUserById failed - invalid token");
                return new { statusCode = 401, message = "Invalid token." };
            }

            int requestingUserId = int.Parse(requestingUserIdClaim);
            if (requestingUserId != id && !currentUser.IsInRole("Admin"))
            {
                _logger.LogWarning("User {RequestingId} tried to access User {TargetId} - Forbidden",
                    requestingUserId, id);
                return new { statusCode = 403, message = "Access denied." };
            }

            var user = _context.Users.Find(id);
            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {Id}", id);
                return new { statusCode = 404, message = "User not found." };
            }

            _logger.LogInformation("User {Id} data returned successfully", id);

            return new
            {
                statusCode = 200,
                data = new UserOutput
                {
                    name = user.name,
                    email = user.email,
                    phone = user.phone
                }
            };
        }

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes);
        }
    }
}