using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ECommerecAPI.Models
{
    public class Order
    {
        [Key]
       
        public int order_Id { get; set; }

        // Foreign Key
        public int UserId { get; set; }

       
        public User ? User { get; set; }
        public DateTime? orderDate { get; set; }


        // many-many relationship with product via OrderProduct
        
        public ICollection<OrderProducts> ? OrderProducts { get; set; } = new List<OrderProducts>();

      
        [NotMapped]
        public decimal TotalAmount =>
       OrderProducts?.Sum(op => op.Quantity * op.Product.Price) ?? 0;







    }
}
