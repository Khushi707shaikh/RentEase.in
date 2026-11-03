using System.ComponentModel.DataAnnotations;

namespace RentEase.DTOs
{
    public class UtilityBillDTO
    {
        [Required]
        public int AgreementId { get; set; }

        [Required]
        [StringLength(50)]
        public string BillType { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000000)]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BillingDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }
    }
}