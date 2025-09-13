using ProjeX.Application.ActualAssignment;

namespace ProjeX.Application.ResourceUtilization
{
    // Type alias for backward compatibility
    public class AssignmentDto : ActualAssignmentDto
    {
    }

    public class UtilizationSummaryDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal TotalAllocationPercent { get; set; }
        public int ProjectCount { get; set; }
        public bool IsOverAllocated { get; set; }
        public bool IsUnderUtilized { get; set; }
        public List<ActualAssignmentDto> Assignments { get; set; } = new();
    }

    public class ProjectUtilizationDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public decimal PlannedCapacity { get; set; }
        public decimal ActualCapacity { get; set; }
        public decimal UtilizationPercentage { get; set; }
        public List<RoleUtilizationDto> RoleUtilization { get; set; } = new();
    }

    public class RoleUtilizationDto
    {
        public string RoleName { get; set; } = string.Empty;
        public decimal TotalAllocation { get; set; }
        public int EmployeeCount { get; set; }
    }

    public class CapacityForecastDto
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal TotalCapacity { get; set; }
        public decimal TotalDemand { get; set; }
        public decimal AvailableCapacity { get; set; }
        public decimal UtilizationPercentage { get; set; }
    }

    public class ResourceRecommendationDto
    {
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public Guid? EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
    }
}

