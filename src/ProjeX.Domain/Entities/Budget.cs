using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class Budget : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? PathId { get; set; }
        public BudgetCategory Category { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal PlannedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal CommittedAmount { get; set; } // Encumbered amount
        public string Currency { get; set; } = "USD";
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public bool IsApproved { get; set; }
        public Guid? ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Additional properties for compatibility
        public decimal AllocatedAmount => PlannedAmount;
        public decimal SpentAmount => ActualAmount;

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual Path? Path { get; set; }
        public virtual Employee? ApprovedBy { get; set; }
    }
}

