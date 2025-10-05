namespace ProjeX.Application.ActualAssignment
{
    /// <summary>
    /// Enhanced validation result for realtime assignment validation with conflict details and suggestions
    /// </summary>
    public class AssignmentValidationResult
    {
        public bool IsValid { get; set; }
        public ValidationSeverity Severity { get; set; } = ValidationSeverity.None;
        public List<string> Messages { get; set; } = new();
        public List<string> BlockingErrors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();

        // Conflict details
        public bool HasConflicts { get; set; }
        public List<ConflictingAssignmentDetail> ConflictingAssignments { get; set; } = new();

        // Availability information
        public decimal RemainingCapacityPercent { get; set; }
        public decimal MaxAllocationFoundPercent { get; set; }
        public DateTime? MaxAllocationDate { get; set; }

        // Suggested windows where assignment can fit
        public List<SuggestedWindow> SuggestedWindows { get; set; } = new();

        // Time-sliced availability (optional, for detailed timeline view)
        public List<DailyAllocation> DailyAllocations { get; set; } = new();

        // Metadata
        public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
        public int ConflictCount => ConflictingAssignments.Count;
    }

    public enum ValidationSeverity
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    /// <summary>
    /// Details of an assignment that conflicts with the requested assignment
    /// </summary>
    public class ConflictingAssignmentDetail
    {
        public Guid AssignmentId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal AllocationPercent { get; set; }
        public string Status { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Suggested date window where the requested allocation can fit
    /// </summary>
    public class SuggestedWindow
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MaxAllocationAvailable { get; set; }
        public int DurationDays { get; set; }
        public string Reason { get; set; } = string.Empty;
        public SuggestionPriority Priority { get; set; }
    }

    public enum SuggestionPriority
    {
        BestFit = 1,      // Closest to requested dates
        Earliest = 2,     // Earliest available
        Longest = 3       // Longest duration
    }

    /// <summary>
    /// Daily allocation snapshot for timeline visualization
    /// </summary>
    public class DailyAllocation
    {
        public DateTime Date { get; set; }
        public decimal TotalAllocationPercent { get; set; }
        public decimal RemainingPercent { get; set; }
        public bool IsOverallocated => TotalAllocationPercent > 100;
    }
}
