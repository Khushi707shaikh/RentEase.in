using System.ComponentModel.DataAnnotations;

namespace RentEase.Models
{
    public class RentPayment
    {
        [Key]
        public int PaymentId { get; set; }

        public int AgreementId { get; set; }
        public RentalAgreement RentalAgreement { get; set; } = null!;

        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public string TransactionReference { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = "Paid";

        public string Remarks { get; set; } = string.Empty;
    }
}