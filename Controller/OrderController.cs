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
        {
            var result = _orderService.GetOrders(User);
            return ToActionResult(result);
        }

        [HttpGet("GetOrderById")]
        public IActionResult GetOrderById(int id)
        {
            var result = _orderService.GetOrderById(id, User);
            return ToActionResult(result);
        }

        private IActionResult ToActionResult(object result)
        {
            var statusCode = (int)result.GetType().GetProperty("statusCode")!.GetValue(result)!;
            return statusCode switch
            {
                200 => Ok(result),
                400 => BadRequest(result),
                401 => Unauthorized(result),
                403 => Forbid(),
                404 => NotFound(result),
                _ => StatusCode(statusCode, result)
            };
        }
    }
}