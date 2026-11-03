using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentEase.Models
{
    public class Property
    {
        [Key]
        public int PropertyId { get; set; }

        [Required]
        [StringLength(150)]
        public string PropertyName { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int TotalRooms { get; set; }

        public int LandlordId { get; set; }

        [ForeignKey("LandlordId")]
        public AppUser Landlord { get; set; } = null!;

        public ICollection<Room> Rooms { get; set; } = [];
    }
}