using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderController> _logger; 

        public OrderController(
            ILogger<OrderController> logger, 
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("GetOrders")]
        public IActionResult GetOrders()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("GetOrders failed - invalid token, no user ID claim");
                return Unauthorized("Invalid token.");
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

            return Ok(output);
        }

        [HttpGet("GetOrderById")]
        public IActionResult GetOrderById(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("GetOrderById failed - invalid token, no user ID claim");
                return Unauthorized("Invalid token.");
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
                return NotFound("Order not found.");
            }

            if (order.UserId != userId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning(
                    "GetOrderById forbidden - User {UserId} tried to access Order {OrderId} owned by User {OwnerId}",
                    userId, id, order.UserId);
                return Forbid();
            }

            _logger.LogInformation(
                "GetOrderById success - Order ID: {OrderId} | Total: {Total} | Date: {Date}",
                order.order_Id, order.TotalAmount, order.orderDate);

            var output = new OrderOutput
            {
                order_Id = order.order_Id,
                orderDate = order.orderDate,
                totalAmount = order.TotalAmount
            };

            return Ok(output);
        }
    }
}