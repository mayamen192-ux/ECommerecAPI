using ECommerecAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderProductController : ControllerBase
    {
        ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost("AddNewOrder")]
        public IActionResult AddNewOrder(Order order)
        {

            try
            {
                decimal totalAmount = 0;

                // Check stock and calculate total
                foreach (var item in order.OrderProducts)
                {
                    var product = db.Products.Find(item.ProductId);

                    if (product == null)
                    {
                        return NotFound($"Product with ID {item.ProductId} not found.");
                    }

                    if (product.Stock < item.Quantity)
                    {
                        return BadRequest(
                            $"Insufficient stock for product {product.Name}. Available: {product.Stock}");
                    }

                    totalAmount += product.Price * item.Quantity;
                }

                // Reduce stock
                foreach (var item in order.OrderProducts)
                {
                    var product = db.Products.Find(item.ProductId);
                    product.Stock -= item.Quantity;
                }

                order.orderDate = DateTime.Now;

                db.Orders.Add(order);
                db.SaveChanges();

                return Ok(new
                {
                    Message = "Order placed successfully",
                    TotalAmount = totalAmount
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
