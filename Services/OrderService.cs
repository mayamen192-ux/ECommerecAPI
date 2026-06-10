using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult GetOrders(ClaimsPrincipal currentUser)
        {
            var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("GetOrders failed - invalid token, no user ID claim");
                return new UnauthorizedObjectResult("Invalid token.");
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

            return new OkObjectResult(output);
        }

        public IActionResult GetOrderById(int id, ClaimsPrincipal currentUser)
        {
            var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("GetOrderById failed - invalid token, no user ID claim");
                return new UnauthorizedObjectResult("Invalid token.");
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
                return new NotFoundObjectResult("Order not found.");
            }

            if (order.UserId != userId && !currentUser.IsInRole("Admin"))
            {
                _logger.LogWarning(
                    "GetOrderById forbidden - User {UserId} tried to access Order {OrderId} owned by User {OwnerId}",
                    userId, id, order.UserId);
                return new ForbidResult();
            }

            _logger.LogInformation(
                "GetOrderById success - Order ID: {OrderId} | Total: {Total} | Date: {Date}",
                order.order_Id, order.TotalAmount, order.orderDate);

            return new OkObjectResult(new OrderOutput
            {
                order_Id = order.order_Id,
                orderDate = order.orderDate,
                totalAmount = order.TotalAmount
            });
        }
    }
}
