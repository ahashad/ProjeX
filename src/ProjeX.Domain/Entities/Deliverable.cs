using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class Deliverable : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? PathId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DeliverableStatus Status { get; set; }
        public Guid? OwnerId { get; set; }
        public int DurationDays { get; set; }
        public int TargetOffsetDays { get; set; } // Offset from project start
        public DateTime AutoExpectedDate { get; set; } // Calculated field
        public string AcceptanceCriteria { get; set; } = string.Empty;
        public string Dependencies { get; set; } = string.Empty;
        public bool IsMilestone { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual Path? Path { get; set; }
        public virtual Employee? Owner { get; set; }
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
        public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();
    }
}


