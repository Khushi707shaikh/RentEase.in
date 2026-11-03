
using System.ComponentModel.DataAnnotations;

namespace RentEase.DTOs
{
    public class AssignRoomDTO
    {
        [Required]
        public int TenantId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime MoveInDate { get; set; }

        [Required]
        [Range(1, 1000000)]
        public decimal MonthlyRent { get; set; }

        [Range(0, 10000000)]
        public decimal SecurityDeposit { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }
}
