using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class Approval : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid DeliverableId { get; set; }
        public ApprovalType Type { get; set; }
        public ApprovalStatus Status { get; set; }
        public Guid? ApproverId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string Comments { get; set; } = string.Empty;
        public string RequiredCriteria { get; set; } = string.Empty;
        public bool IsClientApproval { get; set; }
        public string ClientContact { get; set; } = string.Empty;
        public DateTime? RequestedAt { get; set; }
        public DateTime? DueDate { get; set; }

        // Navigation properties
        public virtual Deliverable Deliverable { get; set; } = null!;
        public virtual Employee? Approver { get; set; }
    }
}

