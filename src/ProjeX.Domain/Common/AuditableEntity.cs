using System; 

namespace ProjeX.Domain.Common
{
    public abstract class AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
        public byte[]? RowVersion { get; set; }
    }
}


