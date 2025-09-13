using ProjeX.Domain.Common;

namespace ProjeX.Domain.Entities
{
    public class PlannedTeamSlot : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public Guid RoleId { get; set; }
        public RolesCatalog Role { get; set; } = null!;
        public decimal PeriodMonths { get; set; }
        public decimal AllocationPercent { get; set; }
        public decimal PlannedSalary { get; set; }
        public decimal PlannedIncentive { get; set; }
        public decimal PlannedCommissionPercent { get; set; }
        public decimal ComputedBudgetCost { get; set; }
    }
}