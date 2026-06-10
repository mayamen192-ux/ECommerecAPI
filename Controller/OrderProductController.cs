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
            => _orderProductService.PlaceOrder(request, User);
    }
}