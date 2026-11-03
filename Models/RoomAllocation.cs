using System.ComponentModel.DataAnnotations;

namespace RentEase.Models
{
    public class RoomAllocation
    {
        [Key]
        public int AllocationId { get; set; }

        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public int RoomId { get; set; }
        public Room Room { get; set; } = null!; 

        public DateTime MoveInDate { get; set; }

        public DateTime? MoveOutDate { get; set; }

        public string Status { get; set; } = "Active";

        public ICollection<RentalAgreement> RentalAgreements { get; set; } = [];
    }
}