using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ECommerecAPI.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Text.Json.Serialization;

    public class Product
    {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]

        
        public decimal  Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int? Stock { get; set; }

        [JsonIgnore]
        public virtual ICollection<Review> ? Reviews { get; set; } = new List<Review>();

        [NotMapped]
        public decimal OverallRating { get; set; }
    }
}
