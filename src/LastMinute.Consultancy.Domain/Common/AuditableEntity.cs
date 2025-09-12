using System; 

namespace LastMinute.Consultancy.Domain.Common
{
    public abstract class AuditableEntity
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
        public byte[]? RowVersion { get; set; }
    }
}


