
using System.ComponentModel.DataAnnotations;

namespace RentEase.DTOs
{
    public class TenantCreateDTO
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;
    }
}
