using System.ComponentModel.DataAnnotations;

namespace ECommerecAPI.DTOs
{
    public class UserRegister
    {
        public string name { get; set; }
        [Required]
        [EmailAddress]
        public string email { get; set; }
        [Required]
        [RegularExpression(
        @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$",
        ErrorMessage = "Password must contain at least 8 characters, one uppercase letter, one lowercase letter, and one number."
    )]

        public string password { get; set; }
        [Required]
        [Phone]
        public string phone { get; set; }
    }
}
