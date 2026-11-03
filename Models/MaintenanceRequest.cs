using System.ComponentModel.DataAnnotations;

namespace RentEase.Models
{
    public class MaintenanceRequest
    {
        [Key]
        public int RequestId { get; set; }

        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public string IssueTitle { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public string Priority { get; set; } = "Medium";

        public string Status { get; set; } = "Pending";

        public DateTime? ResolvedDate { get; set; }
    }
}