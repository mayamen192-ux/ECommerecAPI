namespace ECommerecAPI.DTOs
{
    public class AddReviewDTO
    {
        public int ProId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; }

        public string? Comment { get; set; }
        
    }
}
