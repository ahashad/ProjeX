using ProjeX.Domain.Enums;

namespace ProjeX.Application.ActualAssignment
{
    public class ActualAssignmentDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime ProjectStartDate { get; set; }
        public DateTime ProjectEndDate { get; set; }
        public Guid PlannedTeamSlotId { get; set; }
        public string PlannedTeamSlotDescription { get; set; } = string.Empty;
        public decimal PlannedPeriodMonths { get; set; }
        public decimal PlannedAllocationPercent { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeEmail { get; set; } = string.Empty;
        public string EmployeePhone { get; set; } = string.Empty;
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public decimal AllocationPercent { get; set; }
        public AssignmentStatus Status { get; set; }
        public bool CostCheckWarning { get; set; }
        public decimal CostDifferenceAmount { get; set; }
        public string? RequestedByUserId { get; set; }
        public string? ApprovedByUserId { get; set; }
        public DateTime? ApprovedOn { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

