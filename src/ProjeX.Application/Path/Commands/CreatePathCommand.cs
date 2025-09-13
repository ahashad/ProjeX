using System.ComponentModel.DataAnnotations;

namespace ProjeX.Application.Path.Commands
{
    public class CreatePathCommand
    {
        [Required]
        public Guid ProjectId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string Objective { get; set; } = string.Empty;
        
        [Range(0, 100)]
        public decimal AllowedAllocationPercentage { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal PlannedCost { get; set; }
        
        public Guid? OwnerId { get; set; }
        
        [StringLength(2000)]
        public string Notes { get; set; } = string.Empty;
    }
}

