
using System.ComponentModel.DataAnnotations;

namespace RentEase.DTOs
{
    public class RoomDTO
    {
        [Required]
        public string RoomNumber { get; set; } = string.Empty;

        [Range(1, 1000000)]
        public decimal MonthlyRent { get; set; }

        [Range(1, 20)]
        public int Capacity { get; set; }

        [Required]
        public string RoomType { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Available";

        [Required]
        public int PropertyId { get; set; }
    }
}
