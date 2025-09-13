using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class PlannedTeamSlot : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? PathId { get; set; }
        public Guid RoleId { get; set; }
        public decimal PeriodMonths { get; set; }
        public decimal AllocationPercent { get; set; }
        public decimal PlannedSalary { get; set; }
        public decimal PlannedIncentive { get; set; }
        public decimal PlannedCommissionPercent { get; set; }
        public decimal ComputedBudgetCost { get; set; }
        
        // Enhanced fields for advanced team management
        public decimal PlannedMonthlyCost { get; set; }
        public decimal PlannedVendorCost { get; set; }
        public string Notes { get; set; } = string.Empty;
        public PlannedTeamStatus Status { get; set; } = PlannedTeamStatus.Planned;
        public bool IsVendorSlot { get; set; }
        public string RequiredSkills { get; set; } = string.Empty;
        public int Priority { get; set; } = 1; // 1 = High, 2 = Medium, 3 = Low
        public DateTime? RequiredStartDate { get; set; }
        public DateTime? RequiredEndDate { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual Path? Path { get; set; }
        public virtual RolesCatalog Role { get; set; } = null!;
        public virtual ICollection<ActualAssignment> ActualAssignments { get; set; } = new List<ActualAssignment>();
    }
}