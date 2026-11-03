using System.ComponentModel.DataAnnotations;

namespace RentEase.Models
{
    public class UtilityBill
    {
        [Key]
        public int BillId { get; set; }

        public int AgreementId { get; set; }
        public RentalAgreement RentalAgreement { get; set; } = null!;

        public string BillType { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime BillingDate { get; set; }

        public DateTime DueDate { get; set; }

        public string PaymentStatus { get; set; } = "Pending";
    }
}