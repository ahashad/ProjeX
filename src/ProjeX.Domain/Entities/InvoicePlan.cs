using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class InvoicePlan : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public InvoiceFrequency Frequency { get; set; }
        public decimal TotalContractValue { get; set; }
        public DateTime FirstInvoiceDate { get; set; }
        public DateTime LastInvoiceDate { get; set; }
        public int NumberOfInvoices { get; set; }
        public decimal InvoiceAmount { get; set; }
        public bool IsMilestoneBasedBilling { get; set; }
        public string PaymentTerms { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public decimal TaxRate { get; set; }
        public bool IsActive { get; set; } = true;
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual Project Project { get; set; } = null!;
        public virtual ICollection<InvoiceSchedule> InvoiceSchedules { get; set; } = new List<InvoiceSchedule>();
        public virtual ICollection<BillingRule> BillingRules { get; set; } = new List<BillingRule>();
    }
}

