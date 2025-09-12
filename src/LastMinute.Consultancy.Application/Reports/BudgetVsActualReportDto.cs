namespace LastMinute.Consultancy.Application.Reports
{
    public class BudgetVsActualReportDto
    {
        public List<ProjectBudgetDto> ProjectBudgets { get; set; } = new List<ProjectBudgetDto>();
        public DateTime ReportDate { get; set; }
        public decimal TotalBudgeted { get; set; }
        public decimal TotalActual { get; set; }
        public decimal TotalVariance { get; set; }
        public decimal VariancePercentage { get; set; }
    }

    public class ProjectBudgetDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ProjectStatus { get; set; } = string.Empty;
        public decimal OriginalBudget { get; set; }
        public decimal CurrentBudget { get; set; } // Including approved change requests
        public decimal ActualCost { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercentage { get; set; }
        public decimal LaborCost { get; set; }
        public decimal OverheadCost { get; set; }
        public decimal OtherCosts { get; set; }
        public int TotalHoursWorked { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal CompletionPercentage { get; set; }
    }
}

