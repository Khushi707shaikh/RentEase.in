using System.ComponentModel.DataAnnotations;

namespace RentEase.DTOs
{
    public class ChangePasswordDTO
    {
        [Required]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}