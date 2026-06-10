using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerecAPI.Services
{
    public class OrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            ILogger<OrderService> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public object GetOrders(ClaimsPrincipal currentUser)
        {
            var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("GetOrders failed - invalid token, no user ID claim");
                return new { statusCode = 401, message = "Invalid token." };
            }

            int userId = int.Parse(userIdClaim);
            _logger.LogInformation("GetOrders called by User ID: {UserId}", userId);

            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .ToList();

            var output = orders.Select(o => new OrderOutput
            {
                order_Id = o.order_Id,
                orderDate = o.orderDate,
                totalAmount = o.TotalAmount
            }).ToList();

            _logger.LogInformation("GetOrders returned {Count} orders for User ID: {UserId}",
                output.Count, userId);

            return new { statusCode = 200, data = output };
        }

        public object GetOrderById(int id, ClaimsPrincipal currentUser)
        {
            var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("GetOrderById failed - invalid token, no user ID claim");
                return new { statusCode = 401, message = "Invalid token." };
            }

            int userId = int.Parse(userIdClaim);
            _logger.LogInformation("GetOrderById called for Order ID: {OrderId} by User ID: {UserId}",
                id, userId);

            var order = _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .FirstOrDefault(o => o.order_Id == id);

            if (order == null)
            {
                _logger.LogWarning("GetOrderById failed - order not found: {OrderId}", id);
                return new { statusCode = 404, message = "Order not found." };
            }

            if (order.UserId != userId && !currentUser.IsInRole("Admin"))
            {
                _logger.LogWarning(
                    "GetOrderById forbidden - User {UserId} tried to access Order {OrderId} owned by User {OwnerId}",
                    userId, id, order.UserId);
                return new { statusCode = 403, message = "Access denied." };
            }

            _logger.LogInformation(
                "GetOrderById success - Order ID: {OrderId} | Total: {Total} | Date: {Date}",
                order.order_Id, order.TotalAmount, order.orderDate);

            return new
            {
                statusCode = 200,
                data = new OrderOutput
                {
                    order_Id = order.order_Id,
                    orderDate = order.orderDate,
                    totalAmount = order.TotalAmount
                }
            };
        }
    }
}