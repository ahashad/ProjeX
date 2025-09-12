using LastMinute.Consultancy.Domain.Common;
using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Domain.Entities
{
    public class ActualAssignment : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        // Each PlannedTeamSlot may have multiple ActualAssignments
        public Guid PlannedTeamSlotId { get; set; }
        public PlannedTeamSlot PlannedTeamSlot { get; set; } = null!;
        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal AllocationPercent { get; set; }
        public AssignmentStatus Status { get; set; }
        public bool CostCheckWarning { get; set; }
        public decimal CostDifferenceAmount { get; set; }
        public string? RequestedByUserId { get; set; }
        public string? ApprovedByUserId { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        // Navigation properties
        public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    }
}

