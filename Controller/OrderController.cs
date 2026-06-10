using ECommerecAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("GetOrders")]
        public IActionResult GetOrders()
            => _orderService.GetOrders(User);

        [HttpGet("GetOrderById")]
        public IActionResult GetOrderById(int id)
            => _orderService.GetOrderById(id, User);
    }
}