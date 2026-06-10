using ECommerecAPI.DTOs;
using ECommerecAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewProductController : ControllerBase
    {
        private readonly ReviewProductService _reviewProductService;

        public ReviewProductController(ReviewProductService reviewProductService)
        {
            _reviewProductService = reviewProductService;
        }

        [HttpPost("AddNewReview")]
        public IActionResult AddNewReview([FromBody] AddReviewDTO reviewDto)
            => _reviewProductService.AddNewReview(reviewDto, User);

        [HttpDelete("RemoveReviewById")]
        public IActionResult RemoveReviewById(int id)
            => _reviewProductService.RemoveReviewById(id, User);
    }
}