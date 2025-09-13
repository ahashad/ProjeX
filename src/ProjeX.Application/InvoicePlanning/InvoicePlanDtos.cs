using System.ComponentModel.DataAnnotations;
using ProjeX.Domain.Enums;

namespace ProjeX.Application.InvoicePlanning
{
    public class InvoicePlanDto
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public InvoiceFrequency Frequency { get; set; }
        public string FrequencyName { get; set; } = string.Empty;
        public decimal TotalContractValue { get; set; }
        public DateTime FirstInvoiceDate { get; set; }
        public DateTime LastInvoiceDate { get; set; }
        public int NumberOfInvoices { get; set; }
        public decimal InvoiceAmount { get; set; }
        public bool IsMilestoneBasedBilling { get; set; }
        public string PaymentTerms { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public decimal TaxRate { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<InvoiceScheduleDto> InvoiceSchedules { get; set; } = new();
        public List<BillingRuleDto> BillingRules { get; set; } = new();
    }

    public class CreateInvoicePlanRequest
    {
        [Required]
        public Guid ProjectId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string PlanName { get; set; } = string.Empty;
        
        [Required]
        public InvoiceFrequency Frequency { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalContractValue { get; set; }
        
        [Required]
        public DateTime FirstInvoiceDate { get; set; }
        
        [Required]
        public DateTime LastInvoiceDate { get; set; }
        
        public bool IsMilestoneBasedBilling { get; set; }
        
        [Required]
        [StringLength(100)]
        public string PaymentTerms { get; set; } = "Net 30";
        
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";
        
        [Range(0, 100)]
        public decimal TaxRate { get; set; }
        
        [StringLength(2000)]
        public string Notes { get; set; } = string.Empty;
    }

    public class UpdateInvoicePlanRequest
    {
        [Required]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string PlanName { get; set; } = string.Empty;
        
        [Required]
        public InvoiceFrequency Frequency { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalContractValue { get; set; }
        
        [Required]
        public DateTime FirstInvoiceDate { get; set; }
        
        [Required]
        public DateTime LastInvoiceDate { get; set; }
        
        public bool IsMilestoneBasedBilling { get; set; }
        
        [Required]
        [StringLength(100)]
        public string PaymentTerms { get; set; } = string.Empty;
        
        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = string.Empty;
        
        [Range(0, 100)]
        public decimal TaxRate { get; set; }
        
        [StringLength(2000)]
        public string Notes { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
    }

    public class InvoiceScheduleDto
    {
        public Guid Id { get; set; }
        public Guid InvoicePlanId { get; set; }
        public string InvoicePlanName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public int SequenceNumber { get; set; }
        public DateTime ScheduledDate { get; set; }
        public decimal ScheduledAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid? MilestoneId { get; set; }
        public string MilestoneName { get; set; } = string.Empty;
        public InvoiceScheduleStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public Guid? GeneratedInvoiceId { get; set; }
        public string GeneratedInvoiceNumber { get; set; } = string.Empty;
        public DateTime? ActualInvoiceDate { get; set; }
        public decimal? ActualAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class BillingRuleDto
    {
        public Guid Id { get; set; }
        public Guid InvoicePlanId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public BillingRuleType RuleType { get; set; }
        public string RuleTypeName { get; set; } = string.Empty;
        public string TriggerCondition { get; set; } = string.Empty;
        public decimal? FixedAmount { get; set; }
        public decimal? PercentageOfContract { get; set; }
        public string RateCardReference { get; set; } = string.Empty;
        public bool IncludeTimeEntries { get; set; }
        public bool IncludeExpenses { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class InvoiceGenerationResult
    {
        public bool IsSuccess { get; set; }
        public Guid? InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}

