namespace LastMinute.Consultancy.Application.Reports
{
    public class UtilizationReportDto
    {
        public List<EmployeeUtilizationDto> EmployeeUtilization { get; set; } = new List<EmployeeUtilizationDto>();
        public DateTime ReportStartDate { get; set; }
        public DateTime ReportEndDate { get; set; }
        public decimal OverallUtilizationRate { get; set; }
    }

    public class EmployeeUtilizationDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public decimal TotalHoursWorked { get; set; }
        public decimal TotalAvailableHours { get; set; }
        public decimal UtilizationRate { get; set; }
        public decimal BillableHours { get; set; }
        public decimal NonBillableHours { get; set; }
        public bool IsOnBeach { get; set; } // Not assigned to any active project
        public List<ProjectAssignmentDto> ActiveAssignments { get; set; } = new List<ProjectAssignmentDto>();
    }

    public class ProjectAssignmentDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
    }
}

