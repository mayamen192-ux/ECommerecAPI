using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ECommerecAPI.Models
{
    [PrimaryKey(nameof(OrderId), nameof(ProductId))]
    public class OrderProducts
    {
       
        // Foreign Key
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        // Navigation Property
        public virtual  Order ? Order { get; set; }
      

        // Foreign Key
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        // Navigation Property
        [JsonIgnore]
        public  virtual Product ? Product { get; set; }

        [Required]
        [Range(1, int.MaxValue,
            ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }
    }
}