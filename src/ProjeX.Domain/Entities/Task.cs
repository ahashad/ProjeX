using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class Task : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid DeliverableId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? OwnerId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TaskStatus Status { get; set; }
        public decimal PercentComplete { get; set; }
        public int RemainingDurationDays { get; set; }
        public int EstimatedHours { get; set; }
        public int ActualHours { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Dependencies { get; set; } = string.Empty;

        // Navigation properties
        public virtual Deliverable Deliverable { get; set; } = null!;
        public virtual Employee? Owner { get; set; }
    }
}

