using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderProductController : ControllerBase
    {
        ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost("placeOrder")]
        public IActionResult PlaceOrder(PlaceOrderDTO request)
        {
            var user = db.Users.Find(request.UserId);

            if (user == null)
            {
                return BadRequest("User not found");
            }

            Order order = new Order
            {
                UserId = request.UserId,
                orderDate = DateTime.Now
            };

            db.Orders.Add(order);
            db.SaveChanges();

            List<OrderProducts> orderProducts = new List<OrderProducts>();

            foreach (var item in request.Items)
            {
                var product = db.Products.Find(item.Pid);

                if (product == null)
                {
                    return BadRequest($"Product with ID {item.Pid} does not exist.");
                }

                orderProducts.Add(new OrderProducts
                {
                    OrderId = order.order_Id,
                    ProductId = item.Pid,
                    Quantity = item.qnt
                });
            }

            db.orderProducts.AddRange(orderProducts);
            db.SaveChanges();

            return Ok(new
            {
                Message = "Order placed successfully",
                OrderId = order.order_Id
            });
        }
    }
}