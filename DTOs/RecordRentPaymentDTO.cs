using System.ComponentModel.DataAnnotations;

namespace RentEase.DTOs
{
    public class RecordRentPaymentDTO
    {
        [Required]
        public int AgreementId { get; set; }

        [Required]
        [Range(1, 1000000)]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        public string TransactionReference { get; set; } = string.Empty;

        public string Remarks { get; set; } = string.Empty;
    }
}