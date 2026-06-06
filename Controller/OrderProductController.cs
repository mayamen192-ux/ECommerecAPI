using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderProductController : ControllerBase
    {
        public ApplicationDbContext _context;

        public OrderProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("placeOrder")]
        public IActionResult PlaceOrder(PlaceOrderDTO request)
        {
            // Extract UserId from JWT token — never trust it from the request body
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdClaim);

            var user = _context.Users.Find(userId);
            if (user == null)
                return NotFound("User not found.");

            // Validate ALL products and stock BEFORE saving anything to the DB
            var resolvedItems = new List<(OrderProducts orderProduct, Product product, int quantity)>();
            decimal totalAmount = 0;

            foreach (var item in request.Items)
            {
                var product = _context.Products.Find(item.Pid);

                if (product == null)
                    return NotFound($"Product with ID {item.Pid} does not exist.");

                // Check stock is sufficient before placing the order
                if (product.Stock < item.qnt)
                    return BadRequest($"Insufficient stock for product '{product.Name}'. " +
                                      $"Available: {product.Stock}, Requested: {item.qnt}.");

                // Calculate total amount (price × quantity per item)
                totalAmount += product.Price * item.qnt;

                resolvedItems.Add((
                    new OrderProducts
                    {
                        ProductId = item.Pid,
                        Quantity = item.qnt,
                        Price = product.Price  // Captured at order time
                    },
                    product,
                    item.qnt
                ));
            }

            // Only save the order AFTER all validations pass
            // Note: TotalAmount is a computed property on the Order model —
            // it auto-calculates from OrderProducts, so we do NOT set it here.
            Order order = new Order
            {
                UserId = userId,
                orderDate = DateTime.Now
            };

            _context.Orders.Add(order);
            _context.SaveChanges(); // Save order to get the generated order_Id

            // Attach OrderId and add order products
            List<OrderProducts> orderProducts = new List<OrderProducts>();
            foreach (var (orderProduct, product, quantity) in resolvedItems)
            {
                orderProduct.OrderId = order.order_Id;
                orderProducts.Add(orderProduct);

                //  Reduce stock for each ordered product
                product.Stock -= quantity;
            }

            _context.orderProducts.AddRange(orderProducts);
            _context.SaveChanges(); // Save order products + stock reductions together

            return Ok(new
            {
                Message = "Order placed successfully.",
                OrderId = order.order_Id,
                TotalAmount = totalAmount,
                ItemsOrdered = orderProducts.Count
            });
        }
    }
}