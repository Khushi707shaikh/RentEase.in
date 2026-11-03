using System.ComponentModel.DataAnnotations;

namespace RentEase.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        [Required]
        public string RoomNumber { get; set; } = string.Empty;

        public decimal MonthlyRent { get; set; }

        public int Capacity { get; set; } = 1;

        public string RoomType { get; set; } = string.Empty;

        public string Status { get; set; } = "Available";

        public int PropertyId { get; set; }

        public Property Property { get; set; } = null!;

        public ICollection<RoomAllocation> RoomAllocations { get; set; } = [];
    }
}