namespace RentEase.DTOs
{
    public class TenantDashboardDTO
    {
        public string TenantName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public string PropertyName { get; set; } = string.Empty;

        public string PropertyAddress { get; set; } = string.Empty;

        public string RoomNumber { get; set; } = string.Empty;

        public decimal MonthlyRent { get; set; }

        public DateTime? MoveInDate { get; set; }

        public DateTime? AgreementEndDate { get; set; }
        public decimal TotalRentPaid { get; set; }

        public int PendingUtilityBills { get; set; }
    }
}