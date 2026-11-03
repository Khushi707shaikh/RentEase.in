using System.ComponentModel.DataAnnotations;

namespace RentEase.Models
{
    public class Tenant
    {
        [Key]
        public int TenantId { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string MobileNumber { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int UserId { get; set; }

        public AppUser User { get; set; } = null!;

        public int LandlordId { get; set; }

        public AppUser Landlord { get; set; } = null!;

        public ICollection<RoomAllocation> RoomAllocations { get; set; } = [];
    }
}