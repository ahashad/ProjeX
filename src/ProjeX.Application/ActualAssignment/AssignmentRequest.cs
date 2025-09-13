using System.ComponentModel.DataAnnotations;
using ProjeX.Domain.Enums;

namespace ProjeX.Application.ActualAssignment
{
    public class CreateAssignmentRequest
    {
        [Required]
        public Guid ProjectId { get; set; }
        
        public Guid? PlannedTeamSlotId { get; set; }
        
        [Required]
        public AssigneeType AssigneeType { get; set; }
        
        public Guid? EmployeeId { get; set; }
        
        [StringLength(200)]
        public string VendorName { get; set; } = string.Empty;
        
        [Required]
        [Range(0, 100)]
        public decimal AllocationPercent { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [StringLength(2000)]
        public string Notes { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string CostJustification { get; set; } = string.Empty;
    }

    public class UpdateAssignmentRequest
    {
        [Required]
        public Guid Id { get; set; }
        
        [Required]
        [Range(0, 100)]
        public decimal AllocationPercent { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [StringLength(2000)]
        public string Notes { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string CostJustification { get; set; } = string.Empty;
        
        public AssignmentStatus Status { get; set; }
    }

    public class AssignmentApprovalRequest
    {
        [Required]
        public Guid AssignmentId { get; set; }
        
        [Required]
        public bool IsApproved { get; set; }
        
        [StringLength(1000)]
        public string ApprovalNotes { get; set; } = string.Empty;
    }
}

