using System.ComponentModel.DataAnnotations;

namespace MauiBlazorWebIdentity.Models
{
    public class LoginRequest
    {
        [Required]
        [Display(Name = "Email Address")]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
