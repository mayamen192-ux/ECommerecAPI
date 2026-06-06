using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ECommerecAPI.Models
{
    [PrimaryKey(nameof(OrderId), nameof(ProductId))]
    public class OrderProducts
    {
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        public virtual Order? Order { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }

        public virtual Product? Product { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        // Fixed: Price is now set from Product.Price at order time (not left unset)
        public decimal Price { get; set; }
    }
}
