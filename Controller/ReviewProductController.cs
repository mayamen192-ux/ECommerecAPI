using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // ? All endpoints require authentication
    public class ReviewProductController : ControllerBase
    {
        public ApplicationDbContext _context;

        public ReviewProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("AddNewReview")]
        public IActionResult AddNewReview(AddReviewDTO reviewDto)
        {
            try
            {
                // Extract UserId from JWT token — never trust it from request body
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                    return Unauthorized("Invalid token.");

                int userId = int.Parse(userIdClaim);

                // Verify product exists
                var product = _context.Products.Find(reviewDto.ProId);
                if (product == null)
                    return NotFound("Product not found.");

                // Verify user exists
                var user = _context.Users.Find(userId);
                if (user == null)
                    return NotFound("User not found.");

                // Verify the user has previously ordered this product
                bool hasPurchased = _context.Orders
                    .Where(o => o.UserId == userId)
                    .SelectMany(o => o.OrderProducts)
                    .Any(op => op.ProductId == reviewDto.ProId);

                if (!hasPurchased)
                    return BadRequest("You can only review products you have previously ordered.");

                // Prevent duplicate reviews for the same product
                bool alreadyReviewed = _context.Reviews
                    .Any(r => r.UserId == userId && r.ProductId == reviewDto.ProId);

                if (alreadyReviewed)
                    return BadRequest("You have already reviewed this product.");

                Review review = new Review
                {
                    ProductId = reviewDto.ProId,
                    UserId = userId,          //Use token-extracted userId
                    Rating = reviewDto.Rating,
                    Comment = reviewDto.Comment,
                    ReviewDate = DateTime.Now
                };

                _context.Reviews.Add(review);
                _context.SaveChanges();

                return Ok(new
                {
                    review.Review_Id,
                    review.ProductId,
                    review.UserId,
                    review.Rating,
                    review.Comment,
                    review.ReviewDate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpDelete("RemoveReviewById")]
        public IActionResult RemoveReviewById(int id)
        {
            //  Extract UserId from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdClaim);

            var review = _context.Reviews.Find(id);
            if (review == null)
                return NotFound("Review not found.");

            // Only the review owner can delete it
            if (review.UserId != userId)
                return Forbid();

            _context.Reviews.Remove(review);
            _context.SaveChanges();

            return Ok("Review removed successfully.");
        }
    }
}