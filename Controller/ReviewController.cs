using ECommerecAPI.DTOs;
using ECommerecAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly ReviewService _reviewService;

        public ReviewController(ReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("GetReviewsByProduct")]
        public IActionResult GetReviewsByProduct(
            int productId,
            int pageNumber = 1,
            int pageSize = 5)
            => _reviewService.GetReviewsByProduct(productId, pageNumber, pageSize);

        [HttpPut("UpdateReviewById")]
        public IActionResult UpdateReviewById(int id, [FromBody] UpdatedReviewsDTO dto)
            => _reviewService.UpdateReviewById(id, dto, User);
    }
}