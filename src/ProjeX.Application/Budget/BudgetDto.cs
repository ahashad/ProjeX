using ProjeX.Domain.Enums;

namespace ProjeX.Application.Budget
{
    public class BudgetDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public Guid? PathId { get; set; }
        public string PathName { get; set; } = string.Empty;
        public BudgetCategory Category { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal PlannedAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal CommittedAmount { get; set; }
        public decimal VarianceAmount => ActualAmount - PlannedAmount;
        public decimal VariancePercentage => PlannedAmount > 0 ? (VarianceAmount / PlannedAmount) * 100 : 0;
        public string Currency { get; set; } = "USD";
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public bool IsApproved { get; set; }
        public Guid? ApprovedById { get; set; }
        public string ApprovedByName { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? ModifiedBy { get; set; }
    }
}

