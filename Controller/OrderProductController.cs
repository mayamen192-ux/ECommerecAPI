using ECommerecAPI.DTOs;
using ECommerecAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderProductController : ControllerBase
    {
        private readonly OrderProductService _orderProductService;

        public OrderProductController(OrderProductService orderProductService)
        {
            _orderProductService = orderProductService;
        }

        [HttpPost("placeOrder")]
        public IActionResult PlaceOrder(PlaceOrderDTO request)
        {
            var result = _orderProductService.PlaceOrder(request, User);
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