using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerecAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewProductController : ControllerBase

    {
        ApplicationDbContext db = new ApplicationDbContext();

        [HttpPost("AddNewReview")]
        public IActionResult AddNewReview(AddReviewDTO reviewDto)
        {
            try
            {
                var product = db.Products.Find(reviewDto.ProId);
                if (product == null)
                    return NotFound("Product not found");

                var user = db.Users.Find(reviewDto.UserId);
                if (user == null)
                    return NotFound("User not found");

                Review review = new Review
                {
                    ProductId = reviewDto.ProId,
                    UserId = reviewDto.UserId,
                    Rating = reviewDto.Rating,
                    Comment = reviewDto.Comment,
                    ReviewDate = DateTime.Now
                };

                db.Reviews.Add(review);
                db.SaveChanges();

                //  Return DTO, NOT the raw EF entity
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
        public IActionResult RemoveReviewById(int id, Review review)
        {
            var reviews = db.Reviews.Find(id);

            if (reviews != null)
            {

                db.Reviews.Remove(reviews);

                db.SaveChanges();
                return Ok("Review removed seccessfully");
            }
            return NotFound("Review not found");



        }

        }
    }
