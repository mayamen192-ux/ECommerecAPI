using ECommerecAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        ApplicationDbContext db = new ApplicationDbContext();


        

        [HttpGet("ListAllOrders")]
        public IActionResult ListAllOrders()
        {
            var orders = db.Orders.ToList();

            return Ok(orders);

        }

        [HttpGet("GetOrderById")]
        public IActionResult GetOrderById(int id)
        {

            var order = db.Orders.Find(id);
            if (order != null)
            {

                db.Orders.ToList();
                return Ok(order);
            }

            return NotFound("Order not found");

        }



    }

}
