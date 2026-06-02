using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ECommerecAPI.Models
{
    public class Review
    {
        [Key]

     
        public int Review_Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
     
        public virtual User ? User { get; set; }

        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
     
        public virtual Product  ? Product { get; set; }

        [Required]
        [Range(1, 5,
            ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }
 
        public string? Comment { get; set; }

        public DateTime ReviewDate { get; set; }
            = DateTime.Now;
    }
}