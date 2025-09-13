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
        public Guid? AssignedEmployeeId { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public ProjeX.Domain.Enums.TaskStatus Status { get; set; }
        public int ProgressPercentage { get; set; }
        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public bool IsMilestone { get; set; }
        public int Priority { get; set; } = 3;
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual Deliverable Deliverable { get; set; } = null!;
        public virtual Employee? AssignedEmployee { get; set; }
        // TODO: Add back TaskDependency navigation properties after fixing EF configuration
        // public virtual ICollection<TaskDependency> Dependencies { get; set; } = new List<TaskDependency>();
        // public virtual ICollection<TaskDependency> DependentOn { get; set; } = new List<TaskDependency>();
        public virtual ICollection<Approval> Approvals { get; set; } = new List<Approval>();
    }
}

