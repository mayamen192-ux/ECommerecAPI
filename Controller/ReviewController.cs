using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(
            ILogger<ReviewController> logger, 
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("GetReviewsByProduct")]
        public IActionResult GetReviewsByProduct(
            int productId,
            int pageNumber = 1,
            int pageSize = 5)
        {
            _logger.LogInformation(
                "GetReviewsByProduct called - Product ID: {ProductId} | Page: {Page} | Size: {Size}",
                productId, pageNumber, pageSize);

            var product = _context.Products.Find(productId);
            if (product == null)
            {
                _logger.LogWarning("GetReviewsByProduct failed - product not found: {ProductId}", productId);
                return NotFound("Product not found.");
            }

            var reviews = _context.Reviews
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.ReviewDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.Review_Id,
                    r.UserId,
                    r.ProductId,
                    r.Rating,
                    r.Comment,
                    r.ReviewDate
                })
                .ToList();

            _logger.LogInformation(
                "GetReviewsByProduct returned {Count} reviews for Product ID: {ProductId}",
                reviews.Count, productId);

            return Ok(reviews);
        }

        [HttpPut("UpdateReviewById")]
        public IActionResult UpdateReviewById(int id, [FromBody] UpdatedReviewsDTO dto)
        {
            _logger.LogInformation("UpdateReviewById called for Review ID: {ReviewId}", id);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("UpdateReviewById failed - invalid token, no user ID claim");
                return Unauthorized("Invalid token.");
            }

            int userId = int.Parse(userIdClaim);
            _logger.LogInformation("UpdateReviewById requested by User ID: {UserId}", userId);

            var review = _context.Reviews.Find(id);
            if (review == null)
            {
                _logger.LogWarning("UpdateReviewById failed - review not found: {ReviewId}", id);
                return NotFound("Review not found.");
            }

            if (review.UserId != userId)
            {
                _logger.LogWarning(
                    "UpdateReviewById forbidden - User {UserId} tried to update Review {ReviewId} owned by User {OwnerId}",
                    userId, id, review.UserId);
                return Forbid();
            }

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.ReviewDate = DateTime.Now;

            _context.SaveChanges();

            _logger.LogInformation(
                "Review updated successfully - Review ID: {ReviewId} | Rating: {Rating} | User ID: {UserId}",
                review.Review_Id, review.Rating, userId);

            return Ok(new
            {
                review.Review_Id,
                review.Rating,
                review.Comment,
                review.ReviewDate
            });
        }
    }
}