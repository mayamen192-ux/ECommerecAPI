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
        {
            var result = _reviewService.GetReviewsByProduct(productId, pageNumber, pageSize);
            return ToActionResult(result);
        }

        [HttpPut("UpdateReviewById")]
        public IActionResult UpdateReviewById(int id, [FromBody] UpdatedReviewsDTO dto)
        {
            var result = _reviewService.UpdateReviewById(id, dto, User);
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