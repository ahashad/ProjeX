using System.ComponentModel.DataAnnotations;
using ProjeX.Domain.Enums;

namespace ProjeX.Application.Budget
{
    public class CreateBudgetRequest
    {
        [Required]
        public Guid ProjectId { get; set; }
        
        public Guid? PathId { get; set; }
        
        [Required]
        public BudgetCategory Category { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal PlannedAmount { get; set; }
        
        [StringLength(3)]
        public string Currency { get; set; } = "USD";
        
        [Required]
        public DateTime PeriodStart { get; set; }
        
        [Required]
        public DateTime PeriodEnd { get; set; }
        
        [StringLength(2000)]
        public string Notes { get; set; } = string.Empty;
    }

    public class UpdateBudgetRequest
    {
        [Required]
        public Guid Id { get; set; }
        
        public Guid? PathId { get; set; }
        
        [Required]
        public BudgetCategory Category { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal PlannedAmount { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal ActualAmount { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal CommittedAmount { get; set; }
        
        [StringLength(3)]
        public string Currency { get; set; } = "USD";
        
        [Required]
        public DateTime PeriodStart { get; set; }
        
        [Required]
        public DateTime PeriodEnd { get; set; }
        
        [StringLength(2000)]
        public string Notes { get; set; } = string.Empty;
    }
}

