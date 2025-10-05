using ProjeX.Domain.Enums;

namespace ProjeX.Application.PlannedTeamSlot
{
    public class PlannedTeamSlotDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public decimal PeriodMonths { get; set; }
        public decimal AllocationPercent { get; set; }
        public decimal PlannedSalary { get; set; }
        public decimal PlannedIncentive { get; set; }
        public decimal PlannedCommissionPercent { get; set; }
        public decimal PlannedTickets { get; set; }
        public decimal PlannedHoteling { get; set; }
        public decimal PlannedOthers { get; set; }
        public decimal ComputedBudgetCost { get; set; }
        public bool IsAssigned { get; set; }
        public decimal RemainingAllocationPercent { get; set; }
        public PlannedTeamStatus Status { get; set; } = PlannedTeamStatus.Planned;
        public byte[]? RowVersion { get; set; }

        // Cost aggregates for Grid 1
        public decimal PlannedCost { get; set; }
        public decimal ActualCost { get; set; }
        public decimal Variance { get; set; }

        // Utilization aggregate
        public decimal UtilizationPercent { get; set; }

        // Audit fields
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; }
    }
}