using System.ComponentModel.DataAnnotations;

namespace RentEase.Models
{
    public class TenantDocument
    {
        [Key]
        public int DocumentId { get; set; }

        public int TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public string DocumentName { get; set; } = string.Empty;

        public string DocumentType { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public DateTime UploadedDate { get; set; } = DateTime.Now;
    }
}