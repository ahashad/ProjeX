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

        // Extended properties for team planning dual-grid view
        public decimal UtilizationPercent { get; set; }
        public int DurationDays { get; set; }
        public decimal PlannedCostShare { get; set; }
        public decimal ActualCost { get; set; }
        public decimal CostVariance { get; set; }
        public DateTime TimelineStart { get; set; }
        public DateTime TimelineEnd { get; set; }
        public decimal EmployeeSalary { get; set; }
        public decimal EmployeeMonthlyIncentive { get; set; }
        public decimal EmployeeCommissionPercent { get; set; }

        // Salary snapshot fields - point-in-time cost data for this assignment
        public decimal? SnapshotSalary { get; set; }
        public decimal? SnapshotMonthlyIncentive { get; set; }
        public decimal? SnapshotCommissionPercent { get; set; }
        public decimal? SnapshotTickets { get; set; }
        public decimal? SnapshotHoteling { get; set; }
        public decimal? SnapshotOthers { get; set; }
    }
}

