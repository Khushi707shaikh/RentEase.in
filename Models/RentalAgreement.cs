using System.ComponentModel.DataAnnotations;

namespace RentEase.Models
{
    public class RentalAgreement
    {
        [Key]
        public int AgreementId { get; set; }

        public int AllocationId { get; set; }
        public RoomAllocation RoomAllocation { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal MonthlyRent { get; set; }

        public decimal SecurityDeposit { get; set; }

        public string AgreementStatus { get; set; } = "Active";

        public ICollection<RentPayment> RentPayments { get; set; } =[];

        public ICollection<UtilityBill> UtilityBills { get; set; } = [];
    }
}