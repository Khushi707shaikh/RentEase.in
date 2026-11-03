using System.ComponentModel.DataAnnotations;

namespace RentEase.Models
{
    public class MaintenanceExpense
    {
        [Key]
        public int ExpenseId { get; set; }

        public int PropertyId { get; set; }
        public Property Property { get; set; } = null!;

        public string ExpenseTitle { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; }

        public string PaidTo { get; set; } = string.Empty;
    }
}