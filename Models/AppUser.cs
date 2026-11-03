
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace RentEase.Models
{
    public class AppUser
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
        
        // Landlord or Tenant
        public bool MustChangePassword { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties

        public ICollection<Property> Properties { get; set; } = [];


        public ICollection<Tenant> CreatedTenants { get; set; } = [];

    }
}
