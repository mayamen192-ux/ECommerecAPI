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
        {
            var result = _reviewProductService.AddNewReview(reviewDto, User);
            return ToActionResult(result);
        }

        [HttpDelete("RemoveReviewById")]
        public IActionResult RemoveReviewById(int id)
        {
            var result = _reviewProductService.RemoveReviewById(id, User);
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