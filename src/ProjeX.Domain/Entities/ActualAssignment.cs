using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class ActualAssignment : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? PlannedTeamSlotId { get; set; }
        public Guid? DeliverableId { get; set; } // Direct deliverable assignment
        public Guid? EmployeeId { get; set; }
        public Guid? VendorId { get; set; } // For vendor assignments
        public AssigneeType AssigneeType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal AllocationPercent { get; set; }
        public AssignmentStatus Status { get; set; }
        
        // Enhanced fields for advanced assignment management
        public bool CostCheckWarning { get; set; }
        public decimal CostDifferenceAmount { get; set; }
        public decimal CostDifferencePercentage { get; set; }
        public bool UtilizationWarning { get; set; }
        public bool RoleMismatchWarning { get; set; }
        public string Justification { get; set; } = string.Empty;
        public string? RequestedByUserId { get; set; }
        public string? ApprovedByUserId { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string Notes { get; set; } = string.Empty;
        public int Priority { get; set; } = 1;
        public bool RequiresApproval { get; set; }
        
        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual PlannedTeamSlot? PlannedTeamSlot { get; set; }
        public virtual Deliverable? Deliverable { get; set; }
        public virtual Employee? Employee { get; set; }
        public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    }
}

