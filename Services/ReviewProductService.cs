using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using System.Security.Claims;

namespace ECommerecAPI.Services
{
    public class ReviewProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReviewProductService> _logger;

        public ReviewProductService(
            ILogger<ReviewProductService> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public object AddNewReview(AddReviewDTO reviewDto, ClaimsPrincipal currentUser)
        {
            _logger.LogInformation("AddNewReview called for Product ID: {ProductId}", reviewDto.ProId);

            try
            {
                var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    _logger.LogWarning("AddNewReview failed - invalid token, no user ID claim");
                    return new { statusCode = 401, message = "Invalid token." };
                }

                int userId = int.Parse(userIdClaim);
                _logger.LogInformation("AddNewReview requested by User ID: {UserId}", userId);

                var product = _context.Products.Find(reviewDto.ProId);
                if (product == null)
                {
                    _logger.LogWarning("AddNewReview failed - product not found: {ProductId}", reviewDto.ProId);
                    return new { statusCode = 404, message = "Product not found." };
                }

                var user = _context.Users.Find(userId);
                if (user == null)
                {
                    _logger.LogWarning("AddNewReview failed - user not found: {UserId}", userId);
                    return new { statusCode = 404, message = "User not found." };
                }

                bool hasPurchased = _context.Orders
                    .Where(o => o.UserId == userId)
                    .SelectMany(o => o.OrderProducts)
                    .Any(op => op.ProductId == reviewDto.ProId);

                if (!hasPurchased)
                {
                    _logger.LogWarning(
                        "AddNewReview failed - User {UserId} has not purchased Product {ProductId}",
                        userId, reviewDto.ProId);
                    return new { statusCode = 400, message = "You can only review products you have previously ordered." };
                }

                bool alreadyReviewed = _context.Reviews
                    .Any(r => r.UserId == userId && r.ProductId == reviewDto.ProId);

                if (alreadyReviewed)
                {
                    _logger.LogWarning(
                        "AddNewReview failed - User {UserId} already reviewed Product {ProductId}",
                        userId, reviewDto.ProId);
                    return new { statusCode = 400, message = "You have already reviewed this product." };
                }

                var review = new Review
                {
                    ProductId = reviewDto.ProId,
                    UserId = userId,
                    Rating = reviewDto.Rating,
                    Comment = reviewDto.Comment,
                    ReviewDate = DateTime.Now
                };

                _context.Reviews.Add(review);
                _context.SaveChanges();

                _logger.LogInformation(
                    "Review added successfully - Review ID: {ReviewId} | Product ID: {ProductId} | User ID: {UserId} | Rating: {Rating}",
                    review.Review_Id, review.ProductId, review.UserId, review.Rating);

                return new
                {
                    statusCode = 200,
                    data = new
                    {
                        review.Review_Id,
                        review.ProductId,
                        review.UserId,
                        review.Rating,
                        review.Comment,
                        review.ReviewDate
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in AddNewReview for Product ID: {ProductId}", reviewDto.ProId);
                return new { statusCode = 400, message = ex.ToString() };
            }
        }

        public object RemoveReviewById(int id, ClaimsPrincipal currentUser)
        {
            _logger.LogInformation("RemoveReviewById called for Review ID: {ReviewId}", id);

            var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("RemoveReviewById failed - invalid token, no user ID claim");
                return new { statusCode = 401, message = "Invalid token." };
            }

            int userId = int.Parse(userIdClaim);
            _logger.LogInformation("RemoveReviewById requested by User ID: {UserId}", userId);

            var review = _context.Reviews.Find(id);
            if (review == null)
            {
                _logger.LogWarning("RemoveReviewById failed - review not found: {ReviewId}", id);
                return new { statusCode = 404, message = "Review not found." };
            }

            if (review.UserId != userId)
            {
                _logger.LogWarning(
                    "RemoveReviewById forbidden - User {UserId} tried to delete Review {ReviewId} owned by User {OwnerId}",
                    userId, id, review.UserId);
                return new { statusCode = 403, message = "Access denied." };
            }

            _context.Reviews.Remove(review);
            _context.SaveChanges();

            _logger.LogInformation(
                "Review removed successfully - Review ID: {ReviewId} | User ID: {UserId}", id, userId);

            return new { statusCode = 200, message = "Review removed successfully." };
        }
    }
}