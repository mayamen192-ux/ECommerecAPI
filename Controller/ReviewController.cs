using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // All endpoints require authentication
    public class ReviewController : ControllerBase
    {
        public ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        //  Get reviews filtered by product with pagination (not all reviews globally)
        [HttpGet("GetReviewsByProduct")]
        public IActionResult GetReviewsByProduct(
            int productId,
            int pageNumber = 1,
            int pageSize = 5)
        {
            // Verify product exists
            var product = _context.Products.Find(productId);
            if (product == null)
                return NotFound("Product not found.");

            var reviews = _context.Reviews
                .Where(r => r.ProductId == productId)       // ✅ Filter by product
                .OrderByDescending(r => r.ReviewDate)
                .Skip((pageNumber - 1) * pageSize)          // ✅ Pagination
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

            return Ok(reviews);
        }

        //Update review — only by the user who created it
        [HttpPut("UpdateReviewById")]
        public IActionResult UpdateReviewById(int id, UpdatedReviewsDTO dto)
        {
            //  Extract UserId from JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdClaim);

            var review = _context.Reviews.Find(id);
            if (review == null)
                return NotFound("Review not found.");

            // Only the review owner can update it
            if (review.UserId != userId)
                return Forbid();

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.ReviewDate = DateTime.Now;

            _context.SaveChanges();

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