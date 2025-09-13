using System.ComponentModel.DataAnnotations;
using ProjeX.Domain.Enums;

namespace ProjeX.Application.TaskManagement
{
    public class TaskDto
    {
        public Guid Id { get; set; }
        public Guid DeliverableId { get; set; }
        public string DeliverableName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ProjeX.Domain.Enums.TaskStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public int ProgressPercentage { get; set; }
        public Guid? AssignedEmployeeId { get; set; }
        public string AssignedEmployeeName { get; set; } = string.Empty;
        public bool IsMilestone { get; set; }
        public int Priority { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<TaskDto> DependentTasks { get; set; } = new();
        public List<ApprovalDto> Approvals { get; set; } = new();
    }

    public class CreateTaskRequest
    {
        [Required]
        public Guid DeliverableId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public DateTime PlannedStartDate { get; set; }
        
        [Required]
        public DateTime PlannedEndDate { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal EstimatedHours { get; set; }
        
        public Guid? AssignedEmployeeId { get; set; }
        
        public bool IsMilestone { get; set; }
        
        [Range(1, 5)]
        public int Priority { get; set; } = 3;
        
        [StringLength(2000)]
        public string Notes { get; set; } = string.Empty;
        
        public List<Guid>? DependentTaskIds { get; set; }
    }

    public class UpdateTaskRequest
    {
        [Required]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public ProjeX.Domain.Enums.TaskStatus Status { get; set; }
        
        [Required]
        public DateTime PlannedStartDate { get; set; }
        
        [Required]
        public DateTime PlannedEndDate { get; set; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal EstimatedHours { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal ActualHours { get; set; }
        
        [Range(0, 100)]
        public int ProgressPercentage { get; set; }
        
        public Guid? AssignedEmployeeId { get; set; }
        
        public bool IsMilestone { get; set; }
        
        [Range(1, 5)]
        public int Priority { get; set; } = 3;
        
        [StringLength(2000)]
        public string Notes { get; set; } = string.Empty;
    }

    public class CriticalPathDto
    {
        public Guid DeliverableId { get; set; }
        public List<TaskDto> CriticalTasks { get; set; } = new();
        public decimal TotalDuration { get; set; }
        public DateTime? EarliestCompletionDate { get; set; }
    }

    public class ApprovalDto
    {
        public Guid Id { get; set; }
        public ApprovalType Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public ApprovalStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public Guid? ApprovedById { get; set; }
        public string ApprovedByName { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}

