using ProjeX.Domain.Common;

namespace ProjeX.Domain.Entities
{
    public class TenderDocument : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid TenderId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
      public string Description { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public DateTime UploadedDate { get; set; }
  public Guid? UploadedById { get; set; }

        // Navigation properties
        public virtual Tender Tender { get; set; } = null!;
    public virtual Employee? UploadedBy { get; set; }
    }
}