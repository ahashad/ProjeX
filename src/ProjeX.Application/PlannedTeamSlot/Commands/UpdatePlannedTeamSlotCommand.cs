using System.ComponentModel.DataAnnotations;

namespace ProjeX.Application.PlannedTeamSlot.Commands
{
    public class UpdatePlannedTeamSlotCommand
    {
        public Guid Id { get; set; }
        
        [Required]
        public Guid ProjectId { get; set; }
        
        [Required]
        public Guid RoleId { get; set; }
        
        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Period months must be at least 0.1")]
        public decimal PeriodMonths { get; set; }
        
        [Required]
        [Range(0, 100, ErrorMessage = "Allocation percent must be between 0 and 100")]
        public decimal AllocationPercent { get; set; }
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Planned salary must be positive")]
        public decimal PlannedSalary { get; set; }
        
        [Range(0, double.MaxValue, ErrorMessage = "Planned incentive must be positive")]
        public decimal PlannedIncentive { get; set; }
        
        [Range(0, 100, ErrorMessage = "Planned commission percent must be between 0 and 100")]
        public decimal PlannedCommissionPercent { get; set; }
        
        public byte[] RowVersion { get; set; } = null!;
    }
}