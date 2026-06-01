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
        public IActionResult AddNewReview(Review review)
        {
            db.Reviews.Add(review);
            db.SaveChanges();
            return Ok(" Review added Successfully");
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
