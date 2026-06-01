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
            })
                .ToList();

            return Ok(reviews);

        }

        [HttpPut("UpdateReviewById")]
        public IActionResult UpdateReviewById(int id, Review review)
        {
            var reviews = db.Reviews.Find(id);

            if (reviews != null)
            {
                reviews.ReviewDate = reviews.ReviewDate;
                reviews.Comment = reviews.Comment;
                db.Reviews.Update(reviews);

                db.SaveChanges();
                return Ok("Review updated seccessfully"+id);
            }
            return NotFound("Review not found");



        }

      
    }
}