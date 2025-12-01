using System.ComponentModel.DataAnnotations;

namespace CLDV6212_POE_ST10435542.Models
{
    public class User // this model is used for the Login functionality
    {
        [Key]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; }
    }
}
