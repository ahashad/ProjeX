using System.ComponentModel.DataAnnotations;
using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Application.ActualAssignment.Commands
{
    public class CreateActualAssignmentCommand
    {
        [Required]
        public Guid ProjectId { get; set; }
        
        [Required]
        public Guid PlannedTeamSlotId { get; set; }
        
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Allocation percent must be between 0 and 100")]
        public decimal AllocationPercent { get; set; }
        
        public string Notes { get; set; } = string.Empty;
    }
}

