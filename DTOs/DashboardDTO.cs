namespace RentEase.DTOs
{
    public class DashboardDTO
    {
        public int TotalProperties { get; set; }

        public int TotalRooms { get; set; }

        public int OccupiedRooms { get; set; }

        public int AvailableRooms { get; set; }

        public int TotalTenants { get; set; }

        public decimal TotalRentCollected { get; set; }
        public decimal CurrentMonthRentCollected { get; set; }
    }
}