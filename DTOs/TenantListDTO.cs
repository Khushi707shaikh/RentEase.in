namespace RentEase.DTOs
{
    public class TenantListDTO
    {
        public int TenantId { get; set; }

        public string TenantName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string MobileNumber { get; set; } = string.Empty;

        public string PropertyName { get; set; } = string.Empty;

        public string RoomNumber { get; set; } = string.Empty;

        public decimal MonthlyRent { get; set; }

        public string AgreementStatus { get; set; } = string.Empty;
    }
}