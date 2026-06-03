using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        ApplicationDbContext db = new ApplicationDbContext();

        [HttpGet("ListAllReviews")]
        public IActionResult ListAllReviews()
        {
            var reviews = db.Reviews.Select(r => new
            {
                r.Review_Id,
                r.UserId,
                r.ProductId,
                r.Rating,
                r.Comment,
                r.ReviewDate
            }).ToList();

            return Ok(reviews);
        }

        [HttpPut("UpdateReviewById")]
        public IActionResult UpdateReviewById(int id, UpdatedReviewsDTO dto)
        {
            var review = db.Reviews.Find(id);

            if (review == null)
                return NotFound("Review not found");

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.ReviewDate = DateTime.Now;

            db.SaveChanges();

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