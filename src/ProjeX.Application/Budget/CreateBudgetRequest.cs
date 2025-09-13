using System.ComponentModel.DataAnnotations;
using ProjeX.Domain.Enums;

namespace ProjeX.Application.Budget
{
    public class CreateBudgetRequest
    {
        [Required(ErrorMessage = "Project ID is required")]
        public Guid ProjectId { get; set; }
     
   public Guid? PathId { get; set; }
     
        [Required(ErrorMessage = "Category is required")]
        public BudgetCategory Category { get; set; }
   
        [Required(ErrorMessage = "Description is required")]
        [StringLength(512, ErrorMessage = "Description cannot exceed 512 characters")]
   public string Description { get; set; } = string.Empty;
      
  [Required(ErrorMessage = "Planned amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Planned amount must be positive")]
 public decimal PlannedAmount { get; set; }
 
   [Range(0, double.MaxValue, ErrorMessage = "Actual amount must be positive")]
      public decimal ActualAmount { get; set; }
        
      [Range(0, double.MaxValue, ErrorMessage = "Committed amount must be positive")]
 public decimal CommittedAmount { get; set; }
        
        [StringLength(3, ErrorMessage = "Currency code cannot exceed 3 characters")]
      public string Currency { get; set; } = "USD";
        
   [Required(ErrorMessage = "Period start is required")]
        public DateTime PeriodStart { get; set; }
        
        [Required(ErrorMessage = "Period end is required")]
        public DateTime PeriodEnd { get; set; }
      
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string Notes { get; set; } = string.Empty;
    }

    public class UpdateBudgetRequest
 {
    [Required(ErrorMessage = "ID is required")]
    public Guid Id { get; set; }
    
        [Required(ErrorMessage = "Project ID is required")]
        public Guid ProjectId { get; set; }
   
      public Guid? PathId { get; set; }
        
    [Required(ErrorMessage = "Category is required")]
        public BudgetCategory Category { get; set; }
        
  [Required(ErrorMessage = "Description is required")]
        [StringLength(512, ErrorMessage = "Description cannot exceed 512 characters")]
     public string Description { get; set; } = string.Empty;
       
    [Required(ErrorMessage = "Planned amount is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Planned amount must be positive")]
    public decimal PlannedAmount { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Actual amount must be positive")]
        public decimal ActualAmount { get; set; }
 
  [Range(0, double.MaxValue, ErrorMessage = "Committed amount must be positive")]
        public decimal CommittedAmount { get; set; }
       
        [StringLength(3, ErrorMessage = "Currency code cannot exceed 3 characters")]
        public string Currency { get; set; } = "USD";
        
    [Required(ErrorMessage = "Period start is required")]
        public DateTime PeriodStart { get; set; }
     
        [Required(ErrorMessage = "Period end is required")]
        public DateTime PeriodEnd { get; set; }
        
  [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
   public string Notes { get; set; } = string.Empty;
    }
}