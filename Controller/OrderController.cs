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
    [Authorize] // All endpoints require authentication
    public class OrderController : ControllerBase
    {
        public ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        //  GetOrders — returns ONLY the authenticated user's own orders
        [HttpGet("GetOrders")]
        public IActionResult GetOrders()
        {
            // Extract UserId from JWT token — never trust it from request
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdClaim);

            var orders = _context.Orders
                           .Where(o => o.UserId == userId) // Only this user's orders
                           .Include(o => o.OrderProducts)
                               .ThenInclude(op => op.Product)
                           .ToList();

            var output = orders.Select(o => new OrderOutput
            {
                order_Id = o.order_Id,
                orderDate = o.orderDate,
                totalAmount = o.TotalAmount // Computed automatically from OrderProducts
            }).ToList();

            return Ok(output);
        }

        // GetOrderById — verifies the order belongs to the requesting user
        [HttpGet("GetOrderById")]
        public IActionResult GetOrderById(int id)
        {
            // Extract UserId from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdClaim);

            // Use Include so TotalAmount (computed from OrderProducts) works correctly
            var order = _context.Orders
                          .Include(o => o.OrderProducts)
                              .ThenInclude(op => op.Product)
                          .FirstOrDefault(o => o.order_Id == id);

            if (order == null)
                return NotFound("Order not found.");

            // Only allow the owner to view their order (Admins can view any)
            if (order.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var output = new OrderOutput
            {
                order_Id = order.order_Id,
                orderDate = order.orderDate,
                totalAmount = order.TotalAmount // Computed automatically from OrderProducts
            };

            return Ok(output);
        }
    }
}