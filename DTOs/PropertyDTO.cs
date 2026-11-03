using System.ComponentModel.DataAnnotations;

namespace RentEase.DTOs
{
    public class PropertyDTO
    {
        [Required]
        public string PropertyName { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int TotalRooms { get; set; }
    }
}