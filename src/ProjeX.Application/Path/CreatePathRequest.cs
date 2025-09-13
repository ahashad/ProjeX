using System.ComponentModel.DataAnnotations;

namespace ProjeX.Application.Path
{
    public class CreatePathRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(128, ErrorMessage = "Name cannot exceed 128 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Project ID is required")]
  public Guid ProjectId { get; set; }
        
        [StringLength(512, ErrorMessage = "Objective cannot exceed 512 characters")]
        public string Objective { get; set; } = string.Empty;
   
        [Range(0, 100, ErrorMessage = "Allocation percentage must be between 0 and 100")]
        public decimal AllowedAllocationPercentage { get; set; }
        
   [Range(0, double.MaxValue, ErrorMessage = "Planned cost must be positive")]
        public decimal PlannedCost { get; set; }
        
        public Guid? OwnerId { get; set; }
     
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
     public string Notes { get; set; } = string.Empty;
        
     public bool IsActive { get; set; } = true;
    }

    public class UpdatePathRequest
 {
        [Required(ErrorMessage = "ID is required")]
        public Guid Id { get; set; }
        
    [Required(ErrorMessage = "Name is required")]
        [StringLength(128, ErrorMessage = "Name cannot exceed 128 characters")]
        public string Name { get; set; } = string.Empty;
 
        [Required(ErrorMessage = "Project ID is required")]
        public Guid ProjectId { get; set; }
        
    [StringLength(512, ErrorMessage = "Objective cannot exceed 512 characters")]
        public string Objective { get; set; } = string.Empty;
        
        [Range(0, 100, ErrorMessage = "Allocation percentage must be between 0 and 100")]
        public decimal AllowedAllocationPercentage { get; set; }
        
    [Range(0, double.MaxValue, ErrorMessage = "Planned cost must be positive")]
    public decimal PlannedCost { get; set; }
    
        public Guid? OwnerId { get; set; }
        
      [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string Notes { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
    }
}