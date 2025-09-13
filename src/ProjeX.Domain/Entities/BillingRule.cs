using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class BillingRule : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid InvoicePlanId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public BillingRuleType RuleType { get; set; }
        public string TriggerCondition { get; set; } = string.Empty;
        public decimal? FixedAmount { get; set; }
        public decimal? PercentageOfContract { get; set; }
        public string RateCardReference { get; set; } = string.Empty;
        public bool IncludeTimeEntries { get; set; }
        public bool IncludeExpenses { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; } = true;
        public string Description { get; set; } = string.Empty;

        // Navigation properties
        public virtual InvoicePlan InvoicePlan { get; set; } = null!;
    }
}

