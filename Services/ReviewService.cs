using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerecAPI.Services
{
    public class ReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(
            ILogger<ReviewService> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult GetReviewsByProduct(int productId, int pageNumber, int pageSize)
        {
            _logger.LogInformation(
                "GetReviewsByProduct called - Product ID: {ProductId} | Page: {Page} | Size: {Size}",
                productId, pageNumber, pageSize);

            var product = _context.Products
                .Include(p => p.Reviews)
                .FirstOrDefault(p => p.Id == productId);

            if (product == null)
            {
                _logger.LogWarning("GetReviewsByProduct failed - product not found: {ProductId}", productId);
                return new NotFoundObjectResult("Product not found.");
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

            return new OkObjectResult(new
            {
                ProductId = productId,
                ProductName = product.Name,
                OverallRating = product.OverallRating,
                TotalReviews = reviews.Count,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Reviews = reviews
            });
        }

        public IActionResult UpdateReviewById(int id, UpdatedReviewsDTO dto, ClaimsPrincipal currentUser)
        {
            _logger.LogInformation("UpdateReviewById called for Review ID: {ReviewId}", id);

            var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("UpdateReviewById failed - invalid token, no user ID claim");
                return new UnauthorizedObjectResult("Invalid token.");
            }

            int userId = int.Parse(userIdClaim);
            _logger.LogInformation("UpdateReviewById requested by User ID: {UserId}", userId);

            var review = _context.Reviews.Find(id);
            if (review == null)
            {
                _logger.LogWarning("UpdateReviewById failed - review not found: {ReviewId}", id);
                return new NotFoundObjectResult("Review not found.");
            }

            if (review.UserId != userId)
            {
                _logger.LogWarning(
                    "UpdateReviewById forbidden - User {UserId} tried to update Review {ReviewId} owned by User {OwnerId}",
                    userId, id, review.UserId);
                return new ForbidResult();
            }

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.ReviewDate = DateTime.Now;

            _context.SaveChanges();

            _logger.LogInformation(
                "Review updated successfully - Review ID: {ReviewId} | Rating: {Rating} | User ID: {UserId}",
                review.Review_Id, review.Rating, userId);

            return new OkObjectResult(new
            {
                review.Review_Id,
                review.Rating,
                review.Comment,
                review.ReviewDate
            });
        }
    }
}
